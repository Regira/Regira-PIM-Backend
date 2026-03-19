using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Webshop.Models.Countries;
using Webshop.Services.Entities.Countries;

namespace Webshop.DependencyInjection.Countries;

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