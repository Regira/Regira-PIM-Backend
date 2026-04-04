using PIM.Models.Stakeholders.Parties;

namespace PIM.DataGenerator.Infrastructure;

public class PimDataSeeder(StakeholderSeeder partySeeder, TaxonomySeeder taxonomySeeder, CatalogSeeder catalogSeeder, OrderSeeder orderSeeder)
{

    public async Task SeedAsync()
    {
        var parties = await partySeeder.SeedAsync();
        var facets = await taxonomySeeder.SeedAsync();
        var suppliers = parties.OfType<Organization>().ToList();
        var products = await catalogSeeder.SeedAsync(facets, suppliers);
        await orderSeeder.SeedAsync(parties, products);
    }
}

