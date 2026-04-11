using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PIM.Admin.DependencyInjection;
using PIM.Core.Constants;
using PIM.Data;
using PIM.DataGenerator.Infrastructure;
using PIM.DependencyInjection;
using PIM.Identity.DependencyInjection;
using Regira.Office.Mail.MailGun;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Configuration
        .AddUserSecrets(typeof(Program).Assembly, true);

    builder.Services
        .AddSerilog((_, cfg) => cfg.ReadFrom.Configuration(builder.Configuration));

    builder.Services
        .AddAuthentication();
    builder.Services
        .AddPimAuthentication(builder.Configuration, _ => new MailGunMailer(builder.Configuration.GetSection("MailGun").Get<MailgunConfig>()!));

    builder.Services
        .AddPimServices(builder.Configuration, PimAppTypes.System)
        .AddAdminServices(builder.Configuration)
        .AddTransient<StakeholderSeeder>()
        .AddTransient<TaxonomySeeder>()
        .AddTransient<CatalogSeeder>()
        .AddTransient<OrderSeeder>()
        .AddTransient<PimDataSeeder>();

    var host = builder.Build();

    var logger = host.Services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Starting data seeding...");

    using var scope = host.Services.CreateScope();

    var pimDb = scope.ServiceProvider.GetRequiredService<PimDbContext>();
    await pimDb.Database.EnsureCreatedAsync();

    var entityDbFunctions = new[]
    {
        PartyDbFunctions.CREATE_ALL,
        ProductDbFunctions.CREATE_ALL
    };
    foreach (var dbFunctions in entityDbFunctions)
    {
        foreach (var sql in dbFunctions)
        {
            await pimDb.Database.ExecuteSqlRawAsync(sql);
        }
    }

    if (!pimDb.Orders.Any())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<PimDataSeeder>();
        await seeder.SeedAsync();
    }

    logger.LogInformation("Data seeding completed.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}