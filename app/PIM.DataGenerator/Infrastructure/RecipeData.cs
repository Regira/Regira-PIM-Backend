using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace PIM.DataGenerator.Infrastructure;

public record IngredientEntry(string Name, decimal Quantity);

public record RecipeEntry(string Country, string Dish, IReadOnlyList<IngredientEntry> Ingredients, IReadOnlyList<string> Facets, string Description = "");

public record PartialDishEntry(string Category, string Name, IReadOnlyList<IngredientEntry> Ingredients, IReadOnlyList<string> Facets, string Description = "");

public record FacetCategoryEntry(string Code, string Title, string Description, string FacetGroups = "", string ParentCode = "");

public record FacetGroupEntry(string Code, string Title, string Description);

public static class RecipeDataLoader
{
    public static readonly IReadOnlyDictionary<string, string> CountryRegions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // European
            ["Albania"] = "European", ["Andorra"] = "European", ["Armenia"] = "European",
            ["Austria"] = "European", ["Azerbaijan"] = "European", ["Belarus"] = "European",
            ["Belgium"] = "European", ["Bosnia and Herzegovina"] = "European",
            ["Bulgaria"] = "European", ["Croatia"] = "European", ["Cyprus"] = "European",
            ["Czech Republic"] = "European", ["Denmark"] = "European", ["Estonia"] = "European",
            ["Finland"] = "European", ["France"] = "European", ["Georgia"] = "European",
            ["Germany"] = "European", ["Greece"] = "European", ["Hungary"] = "European",
            ["Iceland"] = "European", ["Ireland"] = "European", ["Italy"] = "European",
            ["Latvia"] = "European", ["Lithuania"] = "European", ["Luxembourg"] = "European",
            ["Malta"] = "European", ["Moldova"] = "European", ["Montenegro"] = "European",
            ["Netherlands"] = "European", ["North Macedonia"] = "European", ["Norway"] = "European",
            ["Poland"] = "European", ["Portugal"] = "European", ["Romania"] = "European",
            ["Russia"] = "European", ["Serbia"] = "European", ["Slovakia"] = "European",
            ["Slovenia"] = "European", ["Spain"] = "European", ["Sweden"] = "European",
            ["Switzerland"] = "European", ["Ukraine"] = "European", ["United Kingdom"] = "European",
            // African
            ["Algeria"] = "African", ["Angola"] = "African", ["Benin"] = "African",
            ["Botswana"] = "African", ["Burkina Faso"] = "African", ["Burundi"] = "African",
            ["Cameroon"] = "African", ["Cape Verde"] = "African",
            ["Central African Republic"] = "African", ["Chad"] = "African",
            ["Comoros"] = "African", ["Congo (DRC)"] = "African", ["Djibouti"] = "African",
            ["Equatorial Guinea"] = "African", ["Eritrea"] = "African", ["Eswatini"] = "African",
            ["Ethiopia"] = "African", ["Gabon"] = "African", ["Gambia"] = "African",
            ["Ghana"] = "African", ["Guinea"] = "African", ["Kenya"] = "African",
            ["Lesotho"] = "African", ["Liberia"] = "African", ["Madagascar"] = "African",
            ["Malawi"] = "African", ["Mali"] = "African", ["Mauritania"] = "African",
            ["Mauritius"] = "African", ["Morocco"] = "African", ["Mozambique"] = "African",
            ["Namibia"] = "African", ["Niger"] = "African", ["Nigeria"] = "African",
            ["Rwanda"] = "African", ["Senegal"] = "African", ["Sierra Leone"] = "African",
            ["Somalia"] = "African", ["South Africa"] = "African", ["Sudan"] = "African",
            ["Tanzania"] = "African", ["Togo"] = "African", ["Tunisia"] = "African",
            ["Uganda"] = "African", ["Zambia"] = "African", ["Zimbabwe"] = "African",
            // Asian
            ["Afghanistan"] = "Asian", ["Bangladesh"] = "Asian", ["Bhutan"] = "Asian",
            ["Brunei"] = "Asian", ["Cambodia"] = "Asian", ["China"] = "Asian",
            ["India"] = "Asian", ["Indonesia"] = "Asian", ["Japan"] = "Asian",
            ["Kazakhstan"] = "Asian", ["Korea (South)"] = "Asian", ["Kyrgyzstan"] = "Asian",
            ["Laos"] = "Asian", ["Malaysia"] = "Asian", ["Maldives"] = "Asian",
            ["Mongolia"] = "Asian", ["Myanmar"] = "Asian", ["Nepal"] = "Asian",
            ["Pakistan"] = "Asian", ["Philippines"] = "Asian", ["Singapore"] = "Asian",
            ["Sri Lanka"] = "Asian", ["Taiwan"] = "Asian", ["Tajikistan"] = "Asian",
            ["Thailand"] = "Asian", ["Timor-Leste"] = "Asian", ["Turkmenistan"] = "Asian",
            ["Uzbekistan"] = "Asian", ["Vietnam"] = "Asian",
            // Middle Eastern
            ["Bahrain"] = "Middle Eastern", ["Egypt"] = "Middle Eastern",
            ["Iran"] = "Middle Eastern", ["Iraq"] = "Middle Eastern",
            ["Israel"] = "Middle Eastern", ["Jordan"] = "Middle Eastern",
            ["Kuwait"] = "Middle Eastern", ["Lebanon"] = "Middle Eastern",
            ["Libya"] = "Middle Eastern", ["Oman"] = "Middle Eastern",
            ["Palestine"] = "Middle Eastern", ["Qatar"] = "Middle Eastern",
            ["Saudi Arabia"] = "Middle Eastern", ["Syria"] = "Middle Eastern",
            ["Turkey"] = "Middle Eastern", ["United Arab Emirates"] = "Middle Eastern",
            ["Yemen"] = "Middle Eastern",
            // Caribbean
            ["Antigua and Barbuda"] = "Caribbean", ["Bahamas"] = "Caribbean",
            ["Barbados"] = "Caribbean", ["Belize"] = "Caribbean", ["Cuba"] = "Caribbean",
            ["Dominica"] = "Caribbean", ["Dominican Republic"] = "Caribbean",
            ["Grenada"] = "Caribbean", ["Guyana"] = "Caribbean", ["Haiti"] = "Caribbean",
            ["Jamaica"] = "Caribbean", ["Saint Lucia"] = "Caribbean",
            ["Trinidad and Tobago"] = "Caribbean",
            // South American
            ["Argentina"] = "South American", ["Bolivia"] = "South American",
            ["Brazil"] = "South American", ["Chile"] = "South American",
            ["Colombia"] = "South American", ["Ecuador"] = "South American",
            ["Paraguay"] = "South American", ["Peru"] = "South American",
            ["Uruguay"] = "South American", ["Venezuela"] = "South American",
            // North/Central American
            ["Canada"] = "North American", ["Costa Rica"] = "North American",
            ["El Salvador"] = "North American", ["Guatemala"] = "North American",
            ["Honduras"] = "North American", ["Mexico"] = "North American",
            ["Nicaragua"] = "North American", ["Panama"] = "North American",
            ["United States"] = "North American",
            // Oceanian
            ["Australia"] = "Oceanian", ["Fiji"] = "Oceanian",
            ["New Zealand"] = "Oceanian", ["Papua New Guinea"] = "Oceanian",
        };

    private record RecipeCsvRow
    {
        public string Country { get; init; } = "";
        public string Dish { get; init; } = "";
        public string Ingredients { get; init; } = "";
        public string Quantities { get; init; } = "";
        public string Facets { get; init; } = "";
        public string Description { get; init; } = "";
    }

    public static IReadOnlyList<RecipeEntry> Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "recipes-per-country.csv");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);

        return csv.GetRecords<RecipeCsvRow>()
            .Select(row =>
            {
                var names = row.Ingredients.Split(';').Select(n => n.Trim()).ToList();
                var quantities = row.Quantities.Split(';').Select(q => q.Trim()).ToList();
                var ingredients = names
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select((n, idx) => new IngredientEntry(n, idx < quantities.Count ? ParseQuantity(quantities[idx]) : 0m))
                    .ToList();
                var facets = row.Facets.Split(';').Select(f => f.Trim()).Where(f => !string.IsNullOrWhiteSpace(f)).ToList();
                return new RecipeEntry(row.Country, row.Dish, ingredients, facets, row.Description);
            })
            .ToList();
    }

    private record PartialDishCsvRow
    {
        public string Category { get; init; } = "";
        public string Name { get; init; } = "";
        public string Ingredients { get; init; } = "";
        public string Quantities { get; init; } = "";
        public string Facets { get; init; } = "";
        public string Description { get; init; } = "";
    }

    public static IReadOnlyList<PartialDishEntry> LoadPartialDishes()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "partial-dishes.csv");

        if (!File.Exists(path)) return [];

        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);

        // Materialise before the reader is disposed
        var rows = csv.GetRecords<PartialDishCsvRow>().ToList();
        return rows
            .Select(row =>
            {
                var names = row.Ingredients.Split(';').Select(n => n.Trim()).ToList();
                var quantities = row.Quantities.Split(';').Select(q => q.Trim()).ToList();

                var ingredients = names
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select((n, idx) => new IngredientEntry(n, idx < quantities.Count ? ParseQuantity(quantities[idx]) : 0m))
                    .ToList();
                var facets = row.Facets.Split(';').Select(f => f.Trim()).Where(f => !string.IsNullOrWhiteSpace(f)).ToList();
                return new PartialDishEntry(row.Category, row.Name, ingredients, facets, row.Description);
            })
            .ToList();
    }

    public static IReadOnlyList<FacetCategoryEntry> LoadFacetCategories()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "facets.csv");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);

        return csv.GetRecords<FacetCategoryEntry>().ToList();
    }

    public static IReadOnlyList<FacetGroupEntry> LoadFacetGroups()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "facet-groups.csv");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);

        return csv.GetRecords<FacetGroupEntry>().ToList();
    }

    private static IngredientEntry ParseIngredientEntry(string part)
    {
        var start = part.IndexOf('(');
        var end   = part.IndexOf(')');

        string name;
        decimal quantity;

        if (start > 0 && end > start)
        {
            name     = part[..start].Trim();
            quantity = ParseQuantity(part[(start + 1)..end].Trim());
        }
        else
        {
            name     = part.Trim();
            quantity = 0m; // sentinel: no explicit quantity given
        }

        return new IngredientEntry(name, quantity);
    }

    /// <summary>
    /// Parses a quantity string from the CSV — e.g. "500g", "1/2 tsp", "3 cloves", "2 tbsp", "1" —
    /// and converts it to the closest SI unit value (grams or millilitres) using standard
    /// cooking-unit conversion factors. Returns 0 when the string is empty or unrecognisable.
    /// </summary>
    private static decimal ParseQuantity(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0m;

        // Special word-only tokens that have no leading digit
        var lower = s.Trim().ToLower();
        if (lower is "pinch" or "a pinch") return 0.5m;   // ~0.5 g
        if (lower is "dash" or "a dash")   return 1m;

        // Extract the leading numeric value (handles mixed fractions, pure fractions, decimals)
        decimal numericValue;
        int endIndex;

        var fractionMatch = System.Text.RegularExpressions.Regex.Match(s,
            @"^(\d+)\s+(\d+)\s*/\s*(\d+)");   // e.g. "1 1/2"
        if (fractionMatch.Success)
        {
            var whole = decimal.Parse(fractionMatch.Groups[1].Value);
            var num   = decimal.Parse(fractionMatch.Groups[2].Value);
            var den   = decimal.Parse(fractionMatch.Groups[3].Value);
            numericValue = whole + num / den;
            endIndex = fractionMatch.Length;
        }
        else
        {
            var pureFractionMatch = System.Text.RegularExpressions.Regex.Match(s,
                @"^(\d+)\s*/\s*(\d+)");        // e.g. "1/2"
            if (pureFractionMatch.Success)
            {
                var num = decimal.Parse(pureFractionMatch.Groups[1].Value);
                var den = decimal.Parse(pureFractionMatch.Groups[2].Value);
                numericValue = den == 0 ? 1m : Math.Round(num / den, 3);
                endIndex = pureFractionMatch.Length;
            }
            else
            {
                var numericMatch = System.Text.RegularExpressions.Regex.Match(s,
                    @"^(\d+(?:\.\d+)?)");       // e.g. "500", "1.5"
                if (numericMatch.Success &&
                    decimal.TryParse(numericMatch.Groups[1].Value,
                        System.Globalization.NumberStyles.Number,
                        System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                {
                    numericValue = parsed;
                    endIndex = numericMatch.Length;
                }
                else
                {
                    return 0m; // unrecognisable
                }
            }
        }

        // Extract the unit token (everything after the number, trimmed and lowercased)
        var unit = s[endIndex..].Trim().ToLower();

        return numericValue * GetUnitMultiplier(unit);
    }

    /// <summary>
    /// Returns the factor needed to convert a CSV cooking-unit quantity to grams or millilitres.
    /// SI units and piece-count strings return 1 (no conversion needed).
    /// </summary>
    private static decimal GetUnitMultiplier(string unit) => unit switch
    {
        // SI units — no conversion
        "g" or "gr" or "gram" or "grams"                     => 1m,
        "ml" or "mls"                                         => 1m,
        // Scale-up SI units
        "kg" or "kgs" or "kilogram" or "kilograms"            => 1000m,
        "l" or "liter" or "liters" or "litre" or "litres"    => 1000m,
        // Cooking volume measures
        "tsp" or "teaspoon" or "teaspoons"                    => 5m,
        "tbsp" or "tablespoon" or "tablespoons"               => 15m,
        "cup" or "cups"                                       => 240m,
        // Aromatics
        "clove" or "cloves"                                   => 3m,    // 1 garlic clove ≈ 3 g
        // Small amounts
        "pinch"                                               => 0.5m,
        "dash"                                                => 1m,
        // Fresh herb measures
        "sprig" or "sprigs"                                   => 2m,    // 1 sprig ≈ 2 g
        "bunch" or "bunches"                                  => 30m,   // 1 bunch ≈ 30 g
        "stalk" or "stalks"                                   => 30m,   // 1 lemongrass stalk ≈ 30 g
        "leaf" or "leaves"                                    => 1m,    // bay leaf / kaffir — counted in pc
        // Everything else (plain numbers, "small", "large", "piece", unknown) — no conversion
        _                                                     => 1m,
    };
}
