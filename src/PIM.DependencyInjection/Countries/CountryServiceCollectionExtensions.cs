using Microsoft.EntityFrameworkCore;
using PIM.Models.Countries;
using PIM.Services.Entities.Countries;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace PIM.DependencyInjection.Countries;

public static class CountryServiceCollectionExtensions
{
    public static IEntityServiceCollection<TContext> AddCountries<TContext>(this IEntityServiceCollection<TContext> services)
        where TContext : DbContext
    {
        services
            .For<Country, string>(e =>
            {
                e.UseEntityService<CountryRepository>();
                //e.AddMapping<CountryDto, CountryDto>();
            });
        return services;
    }
}