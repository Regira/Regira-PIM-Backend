namespace PIM.DataGenerator.Infrastructure;

public record IngredientEntry(string Name, decimal Quantity);

public record RecipeEntry(string Country, string Dish, IReadOnlyList<IngredientEntry> Ingredients);

public record FacetCategoryEntry(string Code, string Title, string Description);

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
            ["Malawi"] = "African",
            // Asian
            ["Afghanistan"] = "Asian", ["Bangladesh"] = "Asian", ["Bhutan"] = "Asian",
            ["Brunei"] = "Asian", ["Cambodia"] = "Asian", ["China"] = "Asian",
            ["India"] = "Asian", ["Indonesia"] = "Asian", ["Japan"] = "Asian",
            ["Kazakhstan"] = "Asian", ["Korea (South)"] = "Asian", ["Kyrgyzstan"] = "Asian",
            ["Laos"] = "Asian", ["Malaysia"] = "Asian", ["Maldives"] = "Asian",
            // Middle Eastern
            ["Bahrain"] = "Middle Eastern", ["Egypt"] = "Middle Eastern",
            ["Iran"] = "Middle Eastern", ["Iraq"] = "Middle Eastern",
            ["Israel"] = "Middle Eastern", ["Jordan"] = "Middle Eastern",
            ["Kuwait"] = "Middle Eastern", ["Lebanon"] = "Middle Eastern",
            ["Libya"] = "Middle Eastern",
            // Caribbean
            ["Antigua and Barbuda"] = "Caribbean", ["Bahamas"] = "Caribbean",
            ["Barbados"] = "Caribbean", ["Belize"] = "Caribbean", ["Cuba"] = "Caribbean",
            ["Dominica"] = "Caribbean", ["Dominican Republic"] = "Caribbean",
            ["Grenada"] = "Caribbean", ["Guyana"] = "Caribbean", ["Haiti"] = "Caribbean",
            ["Jamaica"] = "Caribbean",
            // South American
            ["Argentina"] = "South American", ["Bolivia"] = "South American",
            ["Brazil"] = "South American", ["Chile"] = "South American",
            ["Colombia"] = "South American", ["Ecuador"] = "South American",
            // North/Central American
            ["Canada"] = "North American", ["Costa Rica"] = "North American",
            ["El Salvador"] = "North American", ["Guatemala"] = "North American",
            ["Honduras"] = "North American",
            // Oceanian
            ["Australia"] = "Oceanian", ["Fiji"] = "Oceanian",
        };

    public static IReadOnlyList<RecipeEntry> Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "recipes-per-country.csv");
        var entries = new List<RecipeEntry>();

        using var reader = new StreamReader(path);
        reader.ReadLine(); // skip header

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var fields = ParseCsvLine(line);
            if (fields.Count < 3) continue;

            var ingredients = fields[2]
                .Split(';')
                .Select(i => ParseIngredientEntry(i.Trim()))
                .Where(i => !string.IsNullOrWhiteSpace(i.Name))
                .ToList();

            entries.Add(new RecipeEntry(fields[0].Trim(), fields[1].Trim(), ingredients));
        }

        return entries;
    }

    public static IReadOnlyList<FacetCategoryEntry> LoadFacetCategories()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "facets.csv");
        var entries = new List<FacetCategoryEntry>();

        using var reader = new StreamReader(path);
        reader.ReadLine(); // skip header

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var fields = ParseCsvLine(line);
            if (fields.Count < 3) continue;

            entries.Add(new FacetCategoryEntry(fields[0].Trim(), fields[1].Trim(), fields[2].Trim()));
        }

        return entries;
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
            quantity = 1m;
        }

        return new IngredientEntry(name, quantity);
    }

    /// <summary>
    /// Extracts the leading numeric value (including fractions like 1/2) from a quantity string
    /// such as "500g", "1/2 tsp", "3 cloves", "2 tbsp", "1".
    /// </summary>
    private static decimal ParseQuantity(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 1m;

        // Match a fraction like "1/2" or "3/4" (may have a whole-number prefix like "1 1/2")
        var fractionMatch = System.Text.RegularExpressions.Regex.Match(s,
            @"^(\d+)\s+(\d+)\s*/\s*(\d+)");   // e.g. "1 1/2"
        if (fractionMatch.Success)
        {
            var whole = decimal.Parse(fractionMatch.Groups[1].Value);
            var num   = decimal.Parse(fractionMatch.Groups[2].Value);
            var den   = decimal.Parse(fractionMatch.Groups[3].Value);
            return whole + num / den;
        }

        var pureFractionMatch = System.Text.RegularExpressions.Regex.Match(s,
            @"^(\d+)\s*/\s*(\d+)");            // e.g. "1/2"
        if (pureFractionMatch.Success)
        {
            var num = decimal.Parse(pureFractionMatch.Groups[1].Value);
            var den = decimal.Parse(pureFractionMatch.Groups[2].Value);
            return den == 0 ? 1m : Math.Round(num / den, 3);
        }

        var numericMatch = System.Text.RegularExpressions.Regex.Match(s,
            @"^(\d+(?:\.\d+)?)");              // e.g. "500", "1.5"
        if (numericMatch.Success &&
            decimal.TryParse(numericMatch.Groups[1].Value,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        return 1m;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var sb = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach (var c in line)
        {
            if (c == '"') { inQuotes = !inQuotes; }
            else if (c == ',' && !inQuotes) { fields.Add(sb.ToString()); sb.Clear(); }
            else { sb.Append(c); }
        }
        fields.Add(sb.ToString());
        return fields;
    }
}
