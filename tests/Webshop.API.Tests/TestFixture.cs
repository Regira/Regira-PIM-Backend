using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Services;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Webshop.Data;
using Webshop.DependencyInjection;
using Webshop.Models.Contexts;

namespace Webshop.API.Tests;

public class TestFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    public IServiceProvider ServiceProvider { get; }

    public TestFixture()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IOrderContext, NullOrderContext>();
        services.AddDbContext<WebshopDbContext>((sp, options) =>
            options.UseSqlite(_connection, db => db.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .AddPrimerInterceptors(sp)
                .AddNormalizerInterceptors(sp)
                .AddAutoTruncateInterceptors());
        services.AddEntityServices();

        ServiceProvider = services.BuildServiceProvider();

        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WebshopDbContext>();
        db.Database.EnsureCreated();
    }

    public IServiceScope CreateScope() => ServiceProvider.CreateScope();

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}

file sealed class NullOrderContext : IOrderContext
{
    public int? CustomerId => null;
    public int? OrganizationId => null;
    public DateTime? OrderDate => null;
}
