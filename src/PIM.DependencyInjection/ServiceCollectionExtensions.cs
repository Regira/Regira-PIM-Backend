using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PIM.Core.Abstractions;
using PIM.Core.Constants;
using PIM.Data;
using PIM.DependencyInjection.Catalog;
using PIM.DependencyInjection.Countries;
using PIM.DependencyInjection.Orders;
using PIM.DependencyInjection.Stakeholders;
using PIM.Models.Stakeholders.Parties;
using PIM.Web.Models;
using Regira.DAL.EFcore.Services;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.Mapping.Mapster;

namespace PIM.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPimServices(IConfiguration config, PimAppTypes pimAppType)
        {
            var sqlServerConnectionString = config["ConnectionStrings:SqlServer:PIM"];
            var sqliteConnectionString = config["ConnectionStrings:Sqlite:PIM"];

            services
                .AddHttpContextAccessor()
                // AppContexts
                .AddScoped<IAppContext>(_ => new PimAppContext { AppType = pimAppType })
                .AddScoped<ICultureContext, CultureContext>()
                .AddScoped<IOrderContext, OrderContext>()
                .AddScoped<IUserContext, UserContext>()
                // DbContext
                .AddDbContext<PimDbContext>((sp, options) =>
                {
                    var dbBuilder = !string.IsNullOrWhiteSpace(sqlServerConnectionString)
                        ? options.UseSqlServer(sqlServerConnectionString, db => db.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                        : options.UseSqlite(sqliteConnectionString, db => db.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    dbBuilder
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
                .UseEntities<PimDbContext>(options =>
                {
                    options.UseDefaults();
                    options.UseMapsterMapping(cfg =>
                    {
                        cfg.ForType<Party, PartyDto>()
                            .MapWith(src => (src as Person) != null
                                ? (src as Person).Adapt<PersonDto>()!
                                : (src as Organization).Adapt<OrganizationDto>()!);
                        cfg.ForType<PartyInputDto, Party>()
                            .MapWith(src => (src as PersonInputDto) != null
                                ? (src as PersonInputDto).Adapt<Person>()!
                                : (src as OrganizationInputDto).Adapt<Organization>()!);
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
