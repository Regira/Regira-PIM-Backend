using PIM.Models.Stakeholders.Parties;

namespace PIM.DataGenerator.Infrastructure;

public class PimDataSeeder(StakeholderSeeder partySeeder, TaxonomySeeder taxonomySeeder, UnitTypeSeeder unitTypeSeeder, CatalogSeeder catalogSeeder)
{

    public async Task SeedAsync()
    {
        await unitTypeSeeder.SeedAsync();
        await taxonomySeeder.SeedAsync();
        await catalogSeeder.SeedAsync();
        var parties = await partySeeder.SeedAsync();
        var suppliers = parties.OfType<Organization>().ToList();
    }
}

