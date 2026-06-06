using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PIM.Core.Abstractions;
using PIM.Core.Constants;
using PIM.Data;
using PIM.DependencyInjection.Extensions;
using Regira.DAL.EFcore.Services;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using System.Globalization;

namespace PIM.API.Tests;

public class TestFixture
{
    class FakeAppContext : IAppContext
    {
        public PimAppTypes AppType { get; set; } = PimAppTypes.System;
    }
    class FakeCultureContext : ICultureContext
    {
        public CultureInfo Culture => CultureInfo.InvariantCulture;
        public string LangCode => Culture.TwoLetterISOLanguageName;
        public string? CountryCode => Culture.Name.Split('-').LastOrDefault();

        public Task Init(string? culture)
        {
            return Task.CompletedTask;
        }
    }
    public class FakeUserContext : IUserContext
    {
        public string? UserId => null;
        public ICollection<string> Permissions => [];
    }

    public IServiceProvider ServiceProvider { get; }

    public TestFixture()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddHttpContextAccessor()
            // PIM App Contexts
            .AddScoped<IAppContext, FakeAppContext>()
            .AddScoped<ICultureContext, FakeCultureContext>()
            .AddScoped<IUserContext, FakeUserContext>()
            // Each scope gets its own in-memory SQLite database
            .AddScoped(_ =>
            {
                var connection = new SqliteConnection("Data Source=:memory:");
                connection.Open();
                return connection;
            })
            // DbContext
            .AddDbContext<PimDbContext>((sp, options) =>
            {
                options
                    .UseSqlite(sp.GetRequiredService<SqliteConnection>(), db => db.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                    .AddPrimerInterceptors(sp)
                    .AddNormalizerInterceptors(sp)
                    .AddAutoTruncateInterceptors();
            })
            // Entity Services
            .AddEntityServices(null!);

        ServiceProvider = services.BuildServiceProvider();
    }

    public IServiceScope CreateScope()
    {
        var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PimDbContext>();
        db.Database.EnsureCreated();
        return scope;
    }
}
