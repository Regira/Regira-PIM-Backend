using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Services;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.Mapping.Mapster;
using Webshop.Identity.Data;
using Webshop.Identity.Entities;

namespace Webshop.Admin.DependencyInjection;

public static class AdminServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAdminServices(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("Accounts");

            services
                .AddHttpContextAccessor()
                .AddDbContext<AccountsDbContext>((sp, options) =>
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
                .UseEntities<AccountsDbContext>(options =>
                {
                    options.UseDefaults();
                    options.UseMapsterMapping();
                })
                .AddWebshopUsers();

            return services;
        }
    }
}