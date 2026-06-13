using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PIM.Identity.Data;
using PIM.Identity.DependencyInjection;
using Regira.DAL.EFcore.Services;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
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
                .AddDbContext<AccountsDbContext>((sp, options) =>
                {
                    var dbBuilder = !string.IsNullOrWhiteSpace(sqlServerConnectionString)
                        ? options.UseSqlServer(sqlServerConnectionString, db => db.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                        : options.UseSqlite(sqliteConnectionString, db => db.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    dbBuilder
                        .AddPrimerInterceptors(sp)
                        .AddNormalizerInterceptors(sp)
                        .AddAutoTruncateInterceptors();
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