namespace PIM.DataGenerator.OpenFoodFacts;

/// <summary>
/// Row counts and high-water Ids from an Open Food Facts import run.
/// The Max* values feed the identity reseed, so the application can keep inserting after the load.
/// </summary>
public class OffImportStats
{
    public long Products { get; set; }
    public long IngredientProducts { get; set; }
    public long Components { get; set; }
    public long ProductFacets { get; set; }
    public long ProductSuppliers { get; set; }
    public long Brands { get; set; }
    public long SkippedProducts { get; set; }

    /// <summary>Products dropped because they are not written in the configured language.</summary>
    public long SkippedByLanguage { get; set; }
    public long SkippedCyclicEdges { get; set; }
    public long SkippedOverflowEdges { get; set; }
    public long SkippedDepthEdges { get; set; }
    public long MalformedLines { get; set; }

    /// <summary>Numbers in the dump too large for their column, dropped during the load.</summary>
    public long OutOfRangeValues { get; set; }

    public int MaxProductId { get; set; }
    public int MaxIngredientProductId { get; set; }
    public int MaxBrandPartyId { get; set; }
    public int MaxComponentId { get; set; }
    public int MaxProductFacetId { get; set; }
    public int MaxProductSupplierId { get; set; }

    public long TotalRows =>
        Products + IngredientProducts + Components + ProductFacets + ProductSuppliers + Brands;
}
