using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace PIM.DataGenerator.OpenFoodFacts;

/// <summary>
/// Streams entity instances into SQL Server with <see cref="SqlBulkCopy"/>, deriving the target table,
/// the column names and the CLR-to-column mapping from the EF Core model rather than from hard-coded SQL.
/// Change a mapping in <see cref="PIM.Data.PimDbContext"/> and the importer follows it.
/// </summary>
/// <remarks>
/// Ids are assigned by the importer and written verbatim (<see cref="SqlBulkCopyOptions.KeepIdentity"/>),
/// so that foreign keys can be wired up without a round-trip per row.
/// </remarks>
public sealed class EfBulkWriter<T> : IAsyncDisposable where T : class
{
    private readonly SqlConnection _connection;
    private readonly string _tableName;
    private readonly ColumnMap[] _columns;
    private readonly int _batchSize;
    private readonly int _timeout;
    private DataTable _buffer;

    public long RowsWritten { get; private set; }

    private sealed record ColumnMap(string Name, Type ClrType, Func<T, object?> Getter, int? MaxLength, int? Precision, int? Scale, bool IsNullable);

    /// <summary>Values that did not fit their column's declared precision and were dropped.</summary>
    public long OutOfRangeValues { get; private set; }

    public EfBulkWriter(SqlConnection connection, DbContext context, int batchSize, int timeout)
    {
        _connection = connection;
        _batchSize = batchSize;
        _timeout = timeout;

        var entityType = context.Model.FindEntityType(typeof(T))
            ?? throw new InvalidOperationException($"{typeof(T).Name} is not part of the EF model.");

        var schema = entityType.GetSchema();
        var table = entityType.GetTableName()
            ?? throw new InvalidOperationException($"{typeof(T).Name} is not mapped to a table.");
        _tableName = schema is null ? $"[{table}]" : $"[{schema}].[{table}]";

        var storeObject = StoreObjectIdentifier.Table(table, schema);

        _columns = entityType.GetProperties()
            .Where(p => p.GetColumnName(storeObject) is not null)
            // Computed/generated columns are written by the database, not by us.
            .Where(p => p.ValueGenerated != ValueGenerated.OnAddOrUpdate || p.GetAfterSaveBehavior() == PropertySaveBehavior.Save)
            .Select(p => new ColumnMap(
                p.GetColumnName(storeObject)!,
                Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType,
                BuildGetter(p),
                p.GetMaxLength(),
                DecimalFacets(p).Precision,
                DecimalFacets(p).Scale,
                p.IsNullable))
            .ToArray();

        _buffer = CreateBuffer();
    }

    /// <summary>
    /// Resolves the precision and scale a decimal column is stored with.
    /// </summary>
    /// <remarks>
    /// The precision/scale facets are only populated when the model calls <c>HasPrecision</c>. This one
    /// sets the store type instead (<c>SetDecimalPrecisionConvention</c> yields "decimal(18,4)"), leaving
    /// <c>GetPrecision()</c> and <c>GetScale()</c> null — so the column type is the authoritative source
    /// and the facets are only a fallback.
    /// </remarks>
    private static (int? Precision, int? Scale) DecimalFacets(IProperty property)
    {
        if ((Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType) != typeof(decimal))
            return (null, null);

        if (property.GetPrecision() is { } declared)
            return (declared, property.GetScale() ?? 0);

        var columnType = property.GetColumnType();
        if (columnType is not null)
        {
            var open = columnType.IndexOf('(');
            var close = columnType.IndexOf(')', open + 1);
            if (open > 0 && close > open)
            {
                var parts = columnType[(open + 1)..close].Split(',');
                if (int.TryParse(parts[0].Trim(), out var precision))
                    return (precision, parts.Length > 1 && int.TryParse(parts[1].Trim(), out var scale) ? scale : 0);
            }
        }

        // SQL Server's default when a decimal column is declared without either.
        return (18, 2);
    }

    /// <summary>
    /// Compiles a direct member accessor. EF's own getters live behind internal API, and every mapped
    /// property here is a plain auto-property, so a compiled member access is both public and faster.
    /// </summary>
    private static Func<T, object?> BuildGetter(IProperty property)
    {
        MemberInfo member = (MemberInfo?)property.PropertyInfo ?? property.FieldInfo
            ?? throw new InvalidOperationException($"No CLR member backs '{property.Name}'.");

        var parameter = Expression.Parameter(typeof(T), "e");
        Expression access = Expression.MakeMemberAccess(
            Expression.Convert(parameter, member.DeclaringType!),
            member);

        return Expression.Lambda<Func<T, object?>>(Expression.Convert(access, typeof(object)), parameter).Compile();
    }

    private DataTable CreateBuffer()
    {
        var dt = new DataTable();
        foreach (var column in _columns)
            dt.Columns.Add(column.Name, column.ClrType);
        return dt;
    }

    public async ValueTask AddAsync(T entity)
    {
        var row = _buffer.NewRow();
        foreach (var column in _columns)
            row[column.Name] = Coerce(column, column.Getter(entity));
        _buffer.Rows.Add(row);

        if (_buffer.Rows.Count >= _batchSize)
            await FlushAsync();
    }

    /// <summary>
    /// Applies the constraints the EF model declares, so an over-long OFF title or an out-of-range
    /// quantity becomes a stored value instead of a failed batch.
    /// </summary>
    /// <remarks>
    /// Community-entered data contains genuine nonsense — a product quantity of 312142424824242440000
    /// really is in the dump. Rounding to the column's scale is not enough for those: what breaks the
    /// batch is the number of digits left of the point, so the declared precision has to be checked too.
    /// A value that cannot be represented is dropped rather than silently rescaled into a plausible-looking
    /// but wrong figure.
    /// </remarks>
    private object Coerce(ColumnMap column, object? value)
    {
        switch (value)
        {
            case null:
                return DBNull.Value;

            case string s when column.MaxLength is { } max && s.Length > max:
                return s[..max];

            case decimal d:
            {
                var scale = column.Scale ?? 2;
                var rounded = Math.Round(d, scale, MidpointRounding.AwayFromZero);

                var limit = MaxValueFor(column.Precision ?? 18, scale);
                if (Math.Abs(rounded) <= limit)
                    return rounded;

                OutOfRangeValues++;
                // Nothing sensible can be stored, so record "unknown" where the column allows it and
                // fall back to the largest representable value only where it does not.
                return column.IsNullable
                    ? DBNull.Value
                    : rounded < 0 ? -limit : limit;
            }

            default:
                return value;
        }
    }

    /// <summary>
    /// Largest value a <c>decimal(precision, scale)</c> column can hold.
    /// </summary>
    private static decimal MaxValueFor(int precision, int scale)
    {
        var integerDigits = precision - scale;
        if (integerDigits <= 0)
            return 0m;

        // A CLR decimal tops out around 7.9e28, well below what decimal(38, 0) allows, so anything
        // that wide is already unconstrained from our side.
        if (integerDigits >= 29)
            return decimal.MaxValue;

        var limit = 1m;
        for (var i = 0; i < integerDigits; i++)
            limit *= 10m;

        var smallestUnit = 1m;
        for (var i = 0; i < scale; i++)
            smallestUnit /= 10m;

        return limit - smallestUnit;
    }

    public async Task FlushAsync()
    {
        if (_buffer.Rows.Count == 0)
            return;

        using var bulkCopy = new SqlBulkCopy(_connection, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.TableLock, null)
        {
            DestinationTableName = _tableName,
            BatchSize = _batchSize,
            BulkCopyTimeout = _timeout
        };

        foreach (var column in _columns)
            bulkCopy.ColumnMappings.Add(column.Name, column.Name);

        await bulkCopy.WriteToServerAsync(_buffer);

        RowsWritten += _buffer.Rows.Count;
        _buffer.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        await FlushAsync();
        _buffer.Dispose();
    }
}
