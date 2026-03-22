using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Services;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using System.Globalization;
using Webshop.Core.Abstractions;
using Webshop.Data;
using Webshop.DependencyInjection;
using Webshop.Services.Entities.Catalog.Articles;

namespace Webshop.API.Tests;

public class TestFixture
{
    class FakeCultureContext : ICultureContext
    {
        public CultureInfo Culture => CultureInfo.InvariantCulture;
        public string LangCode => Culture.TwoLetterISOLanguageName;
        public string? CountryCode => Culture.Name.Split('-').LastOrDefault();
        public Task Init()
        {
            return Task.CompletedTask;
        }
    }
    public class FakeOrderContext : IOrderContext
    {
        public int? CustomerId => null;
        public DateTime? OrderDate { get; set; }
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
            // Webshop Contexts
            .AddScoped<ICultureContext, FakeCultureContext>()
            .AddScoped<IOrderContext, FakeOrderContext>()
            .AddScoped<IUserContext, FakeUserContext>()
            // Each scope gets its own in-memory SQLite database
            .AddScoped(_ =>
            {
                var connection = new SqliteConnection("Data Source=:memory:");
                connection.Open();
                return connection;
            })
            // DbContext
            .AddDbContext<WebshopDbContext>((sp, options) =>
            {
                options
                    .UseSqlite(sp.GetRequiredService<SqliteConnection>(), db => db.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                    .AddPrimerInterceptors(sp)
                    .AddNormalizerInterceptors(sp)
                    .AddAutoTruncateInterceptors();
            })
            // Entity Services
            .AddEntityServices()
            // Concrete service registrations for tests
            .AddScoped<ArticleProcessor>();

        ServiceProvider = services.BuildServiceProvider();
    }

    public IServiceScope CreateScope()
    {
        var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WebshopDbContext>();
        db.Database.EnsureCreated();
        return scope;
    }
}
