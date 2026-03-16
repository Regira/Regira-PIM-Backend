using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Services;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.Mapping.Mapster;
using Webshop.Identity.Data;

namespace Webshop.Admin.DependencyInjection;

public static class AdminServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAdminServices(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("Accounts");

            services
                .AddDbContext<WebshopAccountsDbContext>((sp, options) =>
                {
                    options
                        .UseSqlite(connectionString, db => db.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                        .AddPrimerInterceptors(sp)
                        .AddNormalizerInterceptors(sp)
                        .AddAutoTruncateInterceptors();
                })
                .AddHttpContextAccessor()
                .AddEntityServices();
            return services;
        }

        public IServiceCollection AddEntityServices()
        {
            services.AddHttpContextAccessor();

            services
                .UseEntities<WebshopAccountsDbContext>(options =>
                {
                    options.UseDefaults();
                    options.UseMapsterMapping();
                })
                .AddWebshopUsers();

            return services;
        }
    }
}