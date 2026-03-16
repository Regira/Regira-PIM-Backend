using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Services;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.Mapping.Mapster;
using Webshop.Data;
using Webshop.DependencyInjection.Catalog;
using Webshop.DependencyInjection.Orders;
using Webshop.DependencyInjection.Stakeholders;
using Webshop.Models.Contexts;
using Webshop.Models.Entities.Stakeholders.Parties;
using Webshop.Web.Models;

namespace Webshop.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWebshopServices(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("Webshop");

            services
                .AddHttpContextAccessor()
                // OrderContext
                .AddScoped<IOrderContext, OrderContext>()
                .AddDbContext<WebshopDbContext>((sp, options) =>
                {
                    options
                        .UseSqlite(connectionString, db => db.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                        .AddPrimerInterceptors(sp)
                        .AddNormalizerInterceptors(sp)
                        .AddAutoTruncateInterceptors();
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
                .AddCatalog()
                .AddOrders()
                .AddStakeholders();

            return services;
        }
    }
}
