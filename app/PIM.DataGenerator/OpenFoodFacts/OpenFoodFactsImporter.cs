using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PIM.Data;
using PIM.Models.Catalog.Products;
using PIM.Models.Catalog.UnitTypes;
using PIM.Models.Stakeholders.Parties;
using PIM.Models.Taxonomy.FacetGroupFacets;
using PIM.Models.Taxonomy.FacetGroups;
using PIM.Models.Taxonomy.Facets;

namespace PIM.DataGenerator.OpenFoodFacts;

/// <summary>
/// Runs the full Open Food Facts import: download, taxonomy, catalog, then the post-load steps that leave
/// the database usable by the application (constraint re-validation and identity reseed).
/// </summary>
public class OpenFoodFactsImporter(
    PimDbContext dbContext,
    OffDownloader downloader,
    OffTaxonomyImporter taxonomyImporter,
    OffCatalogImporter catalogImporter,
    OffOptions options,
    ILogger<OpenFoodFactsImporter> logger)
{
    /// <summary>Child-before-parent, so the deletes satisfy the foreign keys.</summary>
    private static readonly Type[] DeleteOrder =
    [
        typeof(ProductComponent), typeof(ProductFacet), typeof(ProductSupplier),
        typeof(FacetLink), typeof(FacetParentGroup), typeof(FacetChildGroup),
        typeof(Product), typeof(Facet), typeof(FacetGroup), typeof(UnitType)
    ];

    private static readonly Type[] LoadedTables =
    [
        typeof(Product), typeof(ProductComponent), typeof(ProductFacet), typeof(ProductSupplier),
        typeof(Facet), typeof(FacetLink), typeof(FacetGroup), typeof(FacetParentGroup),
        typeof(UnitType), typeof(Party)
    ];

    public async Task ImportAsync(CancellationToken cancellationToken = default)
    {
        if (!dbContext.Database.IsSqlServer())
            throw new InvalidOperationException(
                "The Open Food Facts import targets SQL Server. Set ConnectionStrings:SqlServer:PIM in appsettings.json or user secrets.");

        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Open Food Facts data is published under the Open Database License (ODbL). " +
                              "A database derived from it stays ODbL: attribute Open Food Facts and keep it open if you publish it.");

        await downloader.DownloadAllAsync(cancellationToken);

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        await using var connection = new SqlConnection(dbContext.Database.GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        await GuardTargetAsync(connection, cancellationToken);

        if (options.TruncateBeforeImport)
            await ClearAsync(connection, cancellationToken);

        if (options.DisableConstraintsDuringLoad)
            await SetConstraintsAsync(connection, enabled: false, cancellationToken);

        logger.LogInformation("Importing taxonomies...");
        var facetIndex = await taxonomyImporter.ImportAsync(connection, dbContext, cancellationToken);

        logger.LogInformation("Importing catalog...");
        var stats = await catalogImporter.ImportAsync(connection, dbContext, facetIndex, cancellationToken);

        // Ad-hoc facets are only known once every product has been seen.
        var nextGroupLinkId = await NextIdAsync<FacetParentGroup>(connection, cancellationToken);
        await taxonomyImporter.WriteAdHocFacetsAsync(connection, dbContext, facetIndex, nextGroupLinkId);

        if (options.DisableConstraintsDuringLoad)
            await SetConstraintsAsync(connection, enabled: true, cancellationToken);

        await ReseedIdentitiesAsync(connection, stats, cancellationToken);

        stopwatch.Stop();
        LogSummary(stats, stopwatch.Elapsed);
    }

    private string TableName(Type clrType)
    {
        var entityType = dbContext.Model.FindEntityType(clrType)
            ?? throw new InvalidOperationException($"{clrType.Name} is not part of the EF model.");

        var schema = entityType.GetSchema();
        var table = entityType.GetTableName()!;
        return schema is null ? $"[{table}]" : $"[{schema}].[{table}]";
    }

    /// <summary>
    /// States which server is about to be written to, and refuses to touch a database that already holds
    /// data unless clearing it was asked for explicitly.
    /// </summary>
    /// <remarks>
    /// The connection string comes from the configuration chain, where user secrets take precedence over
    /// appsettings and environment variables — so the target is not always the one the caller believes they
    /// selected. An import is destructive enough that it must not proceed on that assumption.
    /// </remarks>
    private async Task GuardTargetAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        var target = new SqlConnectionStringBuilder(connection.ConnectionString);
        logger.LogInformation("Import target: {Server} / {Database}", target.DataSource, target.InitialCatalog);

        var productTable = TableName(typeof(Product));
        await using var command = new SqlCommand($"SELECT COUNT(*) FROM (SELECT TOP 1 1 AS x FROM {productTable}) t;", connection)
        {
            CommandTimeout = options.BulkCopyTimeout
        };

        var existing = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        if (existing == 0 || options.TruncateBeforeImport)
            return;

        throw new InvalidOperationException(
            $"'{target.InitialCatalog}' on '{target.DataSource}' already contains products. " +
            "Point the import at an empty database, or set OpenFoodFacts:TruncateBeforeImport to true to " +
            "delete the existing catalog and taxonomy first. Check which server this is before doing so — " +
            "user secrets override appsettings and environment variables in this project.");
    }

    private async Task ClearAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        logger.LogInformation("Clearing existing catalog and taxonomy rows " +
                              "(on a database that already holds a full import, dropping it outright is far quicker)...");

        foreach (var clrType in DeleteOrder)
            await ExecuteAsync(connection, $"DELETE FROM {TableName(clrType)};", cancellationToken);

        // Only the imported brands — Bogus-seeded parties are left alone.
        await ExecuteAsync(connection,
            $"DELETE FROM {TableName(typeof(Party))} WHERE Id >= {options.BrandPartyIdBase};", cancellationToken);
    }

    /// <summary>
    /// Foreign keys are switched off for the load because component rows reference ingredient products that
    /// are written at the end of the pass. Re-enabling with WITH CHECK re-validates every row, which is the
    /// import's own integrity test — if the Id wiring were wrong, this is where it would fail.
    /// </summary>
    private async Task SetConstraintsAsync(SqlConnection connection, bool enabled, CancellationToken cancellationToken)
    {
        logger.LogInformation(enabled ? "Re-enabling and validating foreign keys..." : "Disabling foreign key checks for the load...");

        foreach (var clrType in LoadedTables.Distinct())
        {
            var table = TableName(clrType);
            var sql = enabled
                ? $"ALTER TABLE {table} WITH CHECK CHECK CONSTRAINT ALL;"
                : $"ALTER TABLE {table} NOCHECK CONSTRAINT ALL;";
            await ExecuteAsync(connection, sql, cancellationToken);
        }
    }

    /// <summary>
    /// Bulk copy with KEEPIDENTITY does not advance the identity seed, so without this the application's
    /// next insert would collide with an imported Id.
    /// </summary>
    private async Task ReseedIdentitiesAsync(SqlConnection connection, OffImportStats stats, CancellationToken cancellationToken)
    {
        logger.LogInformation("Reseeding identity columns...");

        var seeds = new (Type ClrType, long Seed)[]
        {
            (typeof(Product), Math.Max(stats.MaxProductId, stats.MaxIngredientProductId)),
            (typeof(ProductComponent), stats.MaxComponentId),
            (typeof(ProductFacet), stats.MaxProductFacetId),
            (typeof(ProductSupplier), stats.MaxProductSupplierId),
            (typeof(Party), stats.MaxBrandPartyId)
        };

        foreach (var (clrType, seed) in seeds)
        {
            if (seed <= 0)
                continue;

            var entityType = dbContext.Model.FindEntityType(clrType)!;
            await ExecuteAsync(connection,
                $"DBCC CHECKIDENT ('{entityType.GetTableName()}', RESEED, {seed});", cancellationToken);
        }

        // Facets and their links are written from contiguous ranges, so the current maximum is the seed.
        foreach (var clrType in new[] { typeof(Facet), typeof(FacetGroup), typeof(FacetLink), typeof(FacetParentGroup), typeof(UnitType) })
        {
            var entityType = dbContext.Model.FindEntityType(clrType)!;
            var table = entityType.GetTableName();
            await ExecuteAsync(connection,
                $"DECLARE @max INT = (SELECT ISNULL(MAX(Id), 0) FROM [{table}]); " +
                $"IF @max > 0 DBCC CHECKIDENT ('{table}', RESEED, @max);", cancellationToken);
        }
    }

    private async Task<int> NextIdAsync<T>(SqlConnection connection, CancellationToken cancellationToken) where T : class
    {
        var table = TableName(typeof(T));
        await using var command = new SqlCommand($"SELECT ISNULL(MAX(Id), 0) FROM {table};", connection);
        command.CommandTimeout = options.BulkCopyTimeout;
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) + 1;
    }

    private async Task ExecuteAsync(SqlConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(sql, connection) { CommandTimeout = options.BulkCopyTimeout };
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private void LogSummary(OffImportStats stats, TimeSpan elapsed)
    {
        logger.LogInformation("""
            Open Food Facts import complete in {Elapsed:hh\:mm\:ss}
              Products (packaged)     {Products,14:N0}
              Products (ingredients)  {Ingredients,14:N0}
              ProductComponent        {Components,14:N0}
              ProductFacet            {ProductFacets,14:N0}
              ProductSupplier         {Suppliers,14:N0}
              Organizations (brands)  {Brands,14:N0}
              ----------------------------------------
              Total rows              {Total,14:N0}
            """,
            elapsed, stats.Products, stats.IngredientProducts, stats.Components,
            stats.ProductFacets, stats.ProductSuppliers, stats.Brands, stats.TotalRows);

        if (stats.SkippedProducts > 0)
            logger.LogInformation("Skipped {Count:N0} products (no barcode, or no ingredients while RequireIngredients is on).", stats.SkippedProducts);
        if (stats.SkippedByLanguage > 0)
            logger.LogInformation("Skipped {Count:N0} products not written in '{Language}'.", stats.SkippedByLanguage, options.Language);
        if (stats.SkippedCyclicEdges > 0)
            logger.LogInformation("Dropped {Count:N0} ingredient links that would have made the component graph cyclic.", stats.SkippedCyclicEdges);
        if (stats.SkippedOverflowEdges > 0)
            logger.LogInformation("Dropped {Count:N0} ingredient links beyond the per-ingredient component cap ({Cap}).", stats.SkippedOverflowEdges, options.MaxComponentsPerIngredient);
        if (stats.SkippedDepthEdges > 0)
            logger.LogInformation("Dropped {Count:N0} ingredient links beyond the ingredient depth cap ({Cap}).", stats.SkippedDepthEdges, options.MaxIngredientDepth);
        if (stats.MalformedLines > 0)
            logger.LogInformation("Skipped {Count:N0} malformed JSONL records.", stats.MalformedLines);
        if (stats.OutOfRangeValues > 0)
            logger.LogInformation("Dropped {Count:N0} numbers too large for their column (community-entered quantities).", stats.OutOfRangeValues);
    }
}
