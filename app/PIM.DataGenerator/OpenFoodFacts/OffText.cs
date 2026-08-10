using System.Globalization;

namespace PIM.DataGenerator.OpenFoodFacts;

/// <summary>
/// Text helpers for turning Open Food Facts tags and labels into presentable PIM titles.
/// </summary>
public static class OffText
{
    /// <summary>
    /// Turns a taxonomy key into a readable title: "fr:pates-a-tartiner" → "Pates a tartiner".
    /// Used when the taxonomy carries no name for the entry, or for tags absent from the taxonomy.
    /// </summary>
    public static string TitleFromKey(string key)
    {
        var value = key;

        // Strip the language prefix ("en:", "fr:", "xx:").
        var colon = value.IndexOf(':');
        if (colon is > 0 and <= 3)
            value = value[(colon + 1)..];

        value = value.Replace('-', ' ').Replace('_', ' ').Trim();

        if (value.Length == 0)
            return key;

        return char.ToUpper(value[0], CultureInfo.InvariantCulture) + value[1..];
    }

    /// <summary>
    /// Picks a display name from a taxonomy entry's multilingual name map, preferring English and
    /// falling back to any other language before the key itself.
    /// </summary>
    public static string DisplayName(string key, IReadOnlyDictionary<string, string>? names)
    {
        if (names is null || names.Count == 0)
            return TitleFromKey(key);

        if (names.TryGetValue("en", out var english) && !string.IsNullOrWhiteSpace(english))
            return english;

        var first = names.Values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        return string.IsNullOrWhiteSpace(first) ? TitleFromKey(key) : first;
    }

    /// <summary>
    /// Joins values into the space-separated string that <c>[Normalized(SourceProperties = [...])]</c>
    /// would have composed, before the normalizer runs over it.
    /// </summary>
    public static string? Join(params string?[] values)
    {
        var parts = values.Where(v => !string.IsNullOrWhiteSpace(v)).ToArray();
        return parts.Length == 0 ? null : string.Join(' ', parts);
    }

    /// <summary>
    /// A product name good enough to show in a list. OFF product names are frequently blank,
    /// in which case the brand and the barcode identify the row.
    /// </summary>
    public static string ProductTitle(OffProduct product)
    {
        if (!string.IsNullOrWhiteSpace(product.ProductName))
            return product.ProductName.Trim();

        if (!string.IsNullOrWhiteSpace(product.GenericName))
            return product.GenericName.Trim();

        var brand = product.BrandsTags?.FirstOrDefault();
        return !string.IsNullOrWhiteSpace(brand)
            ? $"{TitleFromKey(brand)} {product.Code}".Trim()
            : $"Product {product.Code}";
    }
}
