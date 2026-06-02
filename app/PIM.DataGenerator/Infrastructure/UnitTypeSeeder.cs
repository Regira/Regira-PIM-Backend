using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using PIM.Models.Catalog.UnitTypes;
using Regira.Entities.Services.Abstractions;

namespace PIM.DataGenerator.Infrastructure;

public class UnitTypeSeeder(IEntityService<UnitType> unitTypeService, ILogger<UnitTypeSeeder> logger)
{
    private record UnitTypeCsvRow(string Code, string Title, bool IsUom);

    public async Task<IList<UnitType>> SeedAsync()
    {
        var existing = await unitTypeService.List();
        if (existing.Any())
        {
            logger.LogInformation("Unit types already exist, skipping seeding.");
            return existing;
        }

        var rows = ReadFromCsv();
        var types = rows
            .Select(r => new UnitType { Code = r.Code, Title = r.Title, IsUom = r.IsUom })
            .ToList();

        logger.LogInformation("Seeding {Count} unit types...", types.Count);
        foreach (var type in types)
            await unitTypeService.Save(type);
        await unitTypeService.SaveChanges();

        return types;
    }

    private static IReadOnlyList<UnitTypeCsvRow> ReadFromCsv()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "unit-types.csv");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);

        return csv.GetRecords<UnitTypeCsvRow>().ToList();
    }
}
