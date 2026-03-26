using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Services;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.Mapping.Mapster;
using Webshop.Core.Abstractions;
using Webshop.Core.Constants;
using Webshop.Data;
using Webshop.DependencyInjection.Catalog;
using Webshop.DependencyInjection.Countries;
using Webshop.DependencyInjection.Orders;
using Webshop.DependencyInjection.Stakeholders;
using Webshop.Models.Stakeholders.Parties;
using Webshop.Web.Models;

namespace Webshop.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWebshopServices(IConfiguration config, WebshopAppTypes webshopType)
        {
            var connectionString = config.GetConnectionString(WebshopConfig.WebshopDbConnectionStringName);

            services
                .AddHttpContextAccessor()
                // Webshop Contexts
                .AddScoped<IAppContext>(_ => new WebshopAppContext { AppType = webshopType })
                .AddScoped<ICultureContext, CultureContext>()
                .AddScoped<IOrderContext, OrderContext>()
                .AddScoped<IUserContext, UserContext>()
                // DbContext
                .AddDbContext<WebshopDbContext>((sp, options) =>
                {
                    options
                        .UseSqlite(connectionString, db => db.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                        .AddPrimerInterceptors(sp)
                        .AddNormalizerInterceptors(sp)
                        .AddAutoTruncateInterceptors()
                        .ConfigureWarnings(w => w.Ignore(CoreEventId.NavigationBaseIncludeIgnored));
                })
                .AddEntityServices();
            return services;
        }

        public IServiceCollection AddEntityServices()
        {
            services
                .UseEntities<WebshopDbContext>(options =>
                {
                    options.UseDefaults();
                    options.UseMapsterMapping(cfg =>
                    {
                        cfg.ForType<Party, PartyDto>()
                            .MapWith(src => (src as Person) != null
                                ? (src as Person).Adapt<PersonDto>()
                                : (src as Organization).Adapt<OrganizationDto>());
                        cfg.ForType<PartyInputDto, Party>()
                            .MapWith(src => (src as PersonInputDto) != null
                                ? (src as PersonInputDto).Adapt<Person>()
                                : (src as OrganizationInputDto).Adapt<Organization>());
                    });
                })
                .AddCountries()
                .AddCatalog()
                .AddOrders()
                .AddStakeholders();

            return services;
        }
    }
}
