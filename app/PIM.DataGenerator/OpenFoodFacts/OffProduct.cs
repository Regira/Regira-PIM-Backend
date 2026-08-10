using System.Text.Json.Serialization;

namespace PIM.DataGenerator.OpenFoodFacts;

/// <summary>
/// The subset of an Open Food Facts product record the PIM catalog maps onto.
/// The dump carries ~190 fields per product; everything not declared here is skipped while parsing.
/// </summary>
public class OffProduct
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("product_name")]
    public string? ProductName { get; set; }

    [JsonPropertyName("generic_name")]
    public string? GenericName { get; set; }

    /// <summary>
    /// The language the record is written in, so <see cref="ProductName"/> is the name in this language.
    /// </summary>
    [JsonPropertyName("lang")]
    public string? Lang { get; set; }

    /// <summary>Older records carry the language code here instead of in <c>lang</c>.</summary>
    [JsonPropertyName("lc")]
    public string? Lc { get; set; }

    /// <summary>
    /// True when this record is written in <paramref name="language"/> and actually carries a name.
    /// </summary>
    public bool HasNameIn(string language)
        => !string.IsNullOrWhiteSpace(ProductName)
           && string.Equals(Lang ?? Lc, language, StringComparison.OrdinalIgnoreCase);

    /// <summary>Net quantity as printed on the pack, e.g. "350 g".</summary>
    [JsonPropertyName("quantity")]
    public string? Quantity { get; set; }

    /// <summary>Net quantity normalised to a number, e.g. 350.</summary>
    [JsonPropertyName("product_quantity")]
    public decimal? ProductQuantity { get; set; }

    [JsonPropertyName("product_quantity_unit")]
    public string? ProductQuantityUnit { get; set; }

    [JsonPropertyName("brands_tags")]
    public List<string>? BrandsTags { get; set; }

    [JsonPropertyName("categories_tags")]
    public List<string>? CategoriesTags { get; set; }

    [JsonPropertyName("labels_tags")]
    public List<string>? LabelsTags { get; set; }

    [JsonPropertyName("allergens_tags")]
    public List<string>? AllergensTags { get; set; }

    [JsonPropertyName("additives_tags")]
    public List<string>? AdditivesTags { get; set; }

    [JsonPropertyName("food_groups_tags")]
    public List<string>? FoodGroupsTags { get; set; }

    [JsonPropertyName("countries_tags")]
    public List<string>? CountriesTags { get; set; }

    [JsonPropertyName("origins_tags")]
    public List<string>? OriginsTags { get; set; }

    [JsonPropertyName("packaging_materials_tags")]
    public List<string>? PackagingMaterialsTags { get; set; }

    /// <summary>
    /// The parsed ingredient tree. Compound ingredients carry their own nested
    /// <see cref="OffIngredient.Ingredients"/>, which is what gives the catalog its recursion.
    /// </summary>
    [JsonPropertyName("ingredients")]
    public List<OffIngredient>? Ingredients { get; set; }

    [JsonPropertyName("created_t")]
    public long? CreatedT { get; set; }

    [JsonPropertyName("last_modified_t")]
    public long? LastModifiedT { get; set; }

    /// <summary>Every tag list that maps onto a Facet, paired with the taxonomy it belongs to.</summary>
    public IEnumerable<(string Taxonomy, List<string> Tags)> FacetTags()
    {
        if (CategoriesTags is { Count: > 0 }) yield return ("categories", CategoriesTags);
        if (LabelsTags is { Count: > 0 }) yield return ("labels", LabelsTags);
        if (AllergensTags is { Count: > 0 }) yield return ("allergens", AllergensTags);
        if (AdditivesTags is { Count: > 0 }) yield return ("additives", AdditivesTags);
        if (FoodGroupsTags is { Count: > 0 }) yield return ("food_groups", FoodGroupsTags);
        if (CountriesTags is { Count: > 0 }) yield return ("countries", CountriesTags);
        if (OriginsTags is { Count: > 0 }) yield return ("origins", OriginsTags);
        if (PackagingMaterialsTags is { Count: > 0 }) yield return ("packaging_materials", PackagingMaterialsTags);
    }
}

/// <summary>
/// One node of the parsed ingredient tree.
/// </summary>
public class OffIngredient
{
    /// <summary>Taxonomy key, e.g. "en:ricotta". Null for nodes OFF could not resolve.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>The literal text from the label, e.g. "ricotta cheese".</summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("percent_estimate")]
    public decimal? PercentEstimate { get; set; }

    [JsonPropertyName("percent_min")]
    public decimal? PercentMin { get; set; }

    [JsonPropertyName("percent_max")]
    public decimal? PercentMax { get; set; }

    /// <summary>1 when OFF matched the node against its ingredients taxonomy.</summary>
    [JsonPropertyName("is_in_taxonomy")]
    public int? IsInTaxonomy { get; set; }

    /// <summary>Sub-ingredients of a compound ingredient, e.g. "filling" → ricotta, milk.</summary>
    [JsonPropertyName("ingredients")]
    public List<OffIngredient>? Ingredients { get; set; }
}
