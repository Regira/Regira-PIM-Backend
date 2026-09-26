using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models.Abstractions;

namespace PIM.Data;

/// <summary>
/// The PIM database is created with EnsureCreated rather than migrations, which does nothing for a database
/// that already exists. Columns added to the model since are patched in here, idempotently.
/// </summary>
public static class SchemaUpgradeExtensions
{
    const string EmptyGuid = "00000000-0000-0000-0000-000000000000";

    public static async Task UpgradeSchemaAsync(this PimDbContext dbContext, CancellationToken token = default)
    {
        // nothing to upgrade in a database that does not exist yet
        if (!await dbContext.Database.CanConnectAsync(token))
            return;

        await dbContext.AddConcurrencyTokenColumnsAsync(token);
    }

    /// <summary>
    /// Adds the ConcurrencyToken column to every table of an <see cref="IHasConcurrencyToken"/> entity.
    /// Existing rows get an empty token, which the write path treats as "not supplied" — the primer mints
    /// a real one on the next save.
    /// </summary>
    public static async Task AddConcurrencyTokenColumnsAsync(this PimDbContext dbContext, CancellationToken token = default)
    {
        var tables = dbContext.Model.GetEntityTypes()
            // one table per hierarchy (TPH): the root carries the column for every derived type
            .Where(t => t.BaseType == null && typeof(IHasConcurrencyToken).IsAssignableFrom(t.ClrType))
            .Select(t => (Schema: t.GetSchema(), Table: t.GetTableName()!, Column: t.FindProperty(nameof(IHasConcurrencyToken.ConcurrencyToken))!.GetColumnName()))
            .ToList();

        // DDL takes no parameters for identifiers; these come from the EF model, never from input
#pragma warning disable EF1002, EF1003
        foreach (var (schema, table, column) in tables)
        {
            if (dbContext.Database.IsSqlServer())
            {
                var qualified = $"[{schema ?? "dbo"}].[{table}]";
                // a table that does not exist yet is left to EnsureCreated
                await dbContext.Database.ExecuteSqlRawAsync(
                    $"IF OBJECT_ID(N'{qualified}', N'U') IS NOT NULL AND COL_LENGTH(N'{qualified}', N'{column}') IS NULL " +
                    $"ALTER TABLE {qualified} ADD [{column}] uniqueidentifier NOT NULL CONSTRAINT [DF_{table}_{column}] DEFAULT '{EmptyGuid}'",
                    token);
            }
            else if (dbContext.Database.IsSqlite())
            {
                var columns = await dbContext.Database
                    .SqlQuery<string>($"SELECT name AS Value FROM pragma_table_info({table})")
                    .ToListAsync(token);
                // a table that does not exist yet (no columns) is left to EnsureCreated
                if (columns.Count > 0 && !columns.Contains(column))
                {
                    await dbContext.Database.ExecuteSqlRawAsync(
                        $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" TEXT NOT NULL DEFAULT '{EmptyGuid}'",
                        token);
                }
            }
        }
#pragma warning restore EF1002, EF1003
    }
}
