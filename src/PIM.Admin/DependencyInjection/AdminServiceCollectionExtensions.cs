using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PIM.Identity.Data;
using PIM.Identity.DependencyInjection;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;

namespace PIM.Admin.DependencyInjection;

public static class AdminServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAdminServices(IConfiguration config)
        {
            var sqlServerConnectionString = config["ConnectionStrings:SqlServer:Accounts"];
            var sqliteConnectionString = config["ConnectionStrings:Sqlite:Accounts"];

            services
                .AddHttpContextAccessor()
                .AddDbContext<AccountsDbContext>((_, options) =>
                {
                    // Interceptors + UTC convention are auto-wired by UseEntities<AccountsDbContext>().UseDefaults().
                    if (!string.IsNullOrWhiteSpace(sqlServerConnectionString))
                        options.UseSqlServer(sqlServerConnectionString, db => db.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    else
                        options.UseSqlite(sqliteConnectionString, db => db.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                })
                .AddEntityServices(config);
            return services;
        }

        public IServiceCollection AddEntityServices(IConfiguration config)
        {
            services
                .UseEntities<AccountsDbContext>(options =>
                {
                    options.UseDefaults();
                    // allow get all items
                    options.SetPageSize();
                    options.UseMapsterMapping();
                })
                .AddPimUsers();

            return services;
        }
    }
}