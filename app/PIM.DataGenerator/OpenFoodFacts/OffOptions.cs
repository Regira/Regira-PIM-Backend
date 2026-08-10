namespace PIM.DataGenerator.OpenFoodFacts;

/// <summary>
/// Configuration for the Open Food Facts import (appsettings section "OpenFoodFacts").
/// </summary>
public class OffOptions
{
    public const string SectionName = "OpenFoodFacts";

    /// <summary>
    /// Local cache directory for the downloaded dump and taxonomy files.
    /// The JSONL dump is ~12.6 GB compressed, so point this at a disk with room to spare.
    /// </summary>
    public string DataDir { get; set; } = Path.Combine(AppContext.BaseDirectory, "off-data");

    /// <summary>
    /// Base URL for the nightly exports.
    /// </summary>
    public string BaseUrl { get; set; } = "https://static.openfoodfacts.org/data";

    /// <summary>
    /// Skip the download step and use whatever is already in <see cref="DataDir"/>.
    /// Use this when the dump was staged by other means, or to re-run the import offline.
    /// </summary>
    public bool SkipDownload { get; set; }

    /// <summary>
    /// User-Agent sent with every request. Open Food Facts requires a descriptive one in the form
    /// <c>AppName/Version (ContactEmail)</c> and throttles anonymous clients aggressively — put a real
    /// contact address here.
    /// </summary>
    public string UserAgent { get; set; } = "Regira-PIM-DataGenerator/1.0 (please-set-OpenFoodFacts-UserAgent@example.com)";

    /// <summary>
    /// Minimum delay between HTTP requests, to stay inside the per-IP rate limit.
    /// </summary>
    public int RequestDelaySeconds { get; set; } = 4;

    /// <summary>
    /// Attempts per file before giving up, on 429 and other transient responses.
    /// A <c>Retry-After</c> header is honoured; otherwise the delay backs off exponentially.
    /// </summary>
    public int MaxRetries { get; set; } = 6;

    /// <summary>
    /// How long a downloaded taxonomy file is treated as fresh, in hours. A cached file younger than this
    /// is used without contacting the server at all.
    /// </summary>
    /// <remarks>
    /// Resuming the products dump can take several attempts, and re-validating eight taxonomy files on each
    /// one is what pushes a run into the rate limit. The taxonomies are regenerated nightly, so a day of
    /// staleness costs nothing.
    /// </remarks>
    public int TaxonomyMaxAgeHours { get; set; } = 24;

    /// <summary>
    /// Maximum number of packaged products to import. Null imports everything (~4.7 million).
    /// Start with a few hundred thousand to validate the pipeline before committing to a full run.
    /// </summary>
    public int? MaxProducts { get; set; }

    /// <summary>
    /// Rows per <see cref="Microsoft.Data.SqlClient.SqlBulkCopy"/> batch.
    /// </summary>
    public int BatchSize { get; set; } = 50_000;

    /// <summary>
    /// Timeout (seconds) per bulk copy batch.
    /// </summary>
    public int BulkCopyTimeout { get; set; } = 600;

    /// <summary>
    /// Import OFF brands as <see cref="PIM.Models.Stakeholders.Parties.Organization"/> parties and link
    /// them to products via ProductSupplier. When false, Parties are left to the Bogus StakeholderSeeder.
    /// </summary>
    public bool ImportBrands { get; set; } = true;

    /// <summary>
    /// Only create ingredient products for nodes that OFF resolved against its ingredients taxonomy
    /// (<c>is_in_taxonomy = 1</c>). Excluding unresolved nodes keeps free-text parsing noise out of the catalog.
    /// </summary>
    public bool TaxonomyIngredientsOnly { get; set; } = true;

    /// <summary>
    /// Skip products that have no parsed ingredients — they contribute no recursion.
    /// </summary>
    public bool RequireIngredients { get; set; }

    /// <summary>
    /// Keep only products written in this language, e.g. "en". Null imports every language.
    /// </summary>
    /// <remarks>
    /// Matching is on the record's own language (<c>lang</c>/<c>lc</c>), which is what makes
    /// <c>product_name</c> the name in that language — a product also needs a non-empty name to qualify,
    /// since "has an English name" is the point. The per-language <c>product_name_en</c> field looks like
    /// the more direct test but is not: contributors routinely copy the local name into it, so a French
    /// product can carry "Huile d'olive Monini" as its English name.
    /// </remarks>
    public string? Language { get; set; }

    /// <summary>
    /// Maximum sub-components one shared ingredient product may accumulate.
    /// </summary>
    /// <remarks>
    /// Ingredient products are shared catalog-wide, so a generic node like "Flour" would otherwise collect a
    /// child from every product that ever spelled out what its flour contains.
    /// </remarks>
    public int MaxComponentsPerIngredient { get; set; } = 10;

    /// <summary>
    /// Maximum depth of the shared ingredient graph, below a packaged product.
    /// </summary>
    /// <remarks>
    /// No single OFF product nests ingredients more than about four deep, but sharing chains one product's
    /// subtree onto another's and the combined graph runs far deeper. Since the tree functions enumerate
    /// paths rather than nodes, that inflates every offspring query — this caps it back to the shape the
    /// source data actually describes. Raise it for a deliberately punishing recursion test.
    /// </remarks>
    public int MaxIngredientDepth { get; set; } = 3;

    /// <summary>
    /// Disable foreign key constraint checking during the load and re-validate afterwards.
    /// Required for single-pass streaming, since component rows are written before the
    /// ingredient products they reference.
    /// </summary>
    public bool DisableConstraintsDuringLoad { get; set; } = true;

    /// <summary>
    /// Delete existing catalog/taxonomy rows before importing.
    /// </summary>
    /// <remarks>
    /// Off by default, deliberately. The import resolves its connection string through the normal
    /// configuration chain, and in this project user secrets are applied last — so the server actually
    /// written to is not always the one named in appsettings or in an environment variable. An import
    /// into a non-empty database aborts unless this is explicitly turned on.
    /// </remarks>
    public bool TruncateBeforeImport { get; set; }

    /// <summary>
    /// First Id assigned to packaged products.
    /// </summary>
    public int ProductIdBase { get; set; } = 1;

    /// <summary>
    /// First Id assigned to ingredient products. Ingredient products live in their own Id range so both
    /// kinds can be emitted in a single streaming pass without a pre-scan.
    /// </summary>
    public int IngredientProductIdBase { get; set; } = 100_000_000;

    /// <summary>
    /// First Id assigned to facets discovered on a product but absent from the published taxonomy
    /// (OFF products routinely carry locale-specific tags such as "fr:pates-a-tartiner").
    /// </summary>
    public int AdHocFacetIdBase { get; set; } = 1_000_000;

    /// <summary>
    /// First Id assigned to brand Organizations. Kept clear of the Bogus StakeholderSeeder's range so the
    /// two can coexist; identity seeds are reset after the import either way.
    /// </summary>
    public int BrandPartyIdBase { get; set; } = 1_000_000;

    /// <summary>
    /// Taxonomies imported as FacetGroups. Each becomes one FacetGroup; each entry becomes one Facet,
    /// and its <c>parents</c> become FacetLink rows.
    /// </summary>
    public string[] Taxonomies { get; set; } =
    [
        "categories",
        "labels",
        "allergens",
        "additives",
        "food_groups",
        "countries",
        "origins",
        "packaging_materials"
    ];
}
