using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PIM.Admin.DependencyInjection;
using PIM.Core.Constants;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PIM.Data;
using PIM.DataGenerator.Infrastructure;
using PIM.DataGenerator.OpenFoodFacts;
using PIM.DependencyInjection.Extensions;
using PIM.Identity.DependencyInjection;
using Regira.Licensing.DependencyInjection;
using Regira.Normalizing;
using Regira.Normalizing.Abstractions;
using Regira.Office.Mail.MailGun;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // Source selection: the hand-built recipe catalog, or the Open Food Facts dump.
    var useOpenFoodFacts = args.Contains("--off", StringComparer.OrdinalIgnoreCase)        || string.Equals(builder.Configuration["DataSource"], "OpenFoodFacts", StringComparison.OrdinalIgnoreCase);

    builder.Configuration
        .AddUserSecrets(typeof(Program).Assembly, true);

    builder.Services
        .AddSerilog((_, cfg) => cfg.ReadFrom.Configuration(builder.Configuration));

    builder.Services
        .AddAuthentication();
    builder.Services
        .AddPimAuthentication(builder.Configuration, _ => new MailGunMailer(builder.Configuration.GetSection("MailGun").Get<MailgunConfig>()!));

    builder.Services
        .UseRegira(builder.Configuration["Regira:LicenseKey"])
        .AddPimServices(builder.Configuration, PimAppTypes.System)
        .AddAdminServices(builder.Configuration)
        .AddTransient<StakeholderSeeder>()
        .AddTransient<TaxonomySeeder>()
        .AddTransient<UnitTypeSeeder>()
        .AddTransient<CatalogSeeder>()
        .AddTransient<PimDataSeeder>();

    // Open Food Facts import
    builder.Services
        .AddSingleton(builder.Configuration.GetSection(OffOptions.SectionName).Get<OffOptions>() ?? new OffOptions())
        .AddHttpClient<OffDownloader>(client => client.Timeout = Timeout.InfiniteTimeSpan);
    builder.Services
        .AddTransient<OffJsonlReader>()
        .AddTransient<OffTaxonomyImporter>()
        .AddTransient<OffCatalogImporter>()
        .AddTransient<OpenFoodFactsImporter>();
    // Registered by UseEntities().UseDefaults(); kept as a fallback so the importer can normalize
    // the fields the entity services would otherwise have filled in.
    builder.Services.TryAddTransient<INormalizer, DefaultNormalizer>();

    var host = builder.Build();

    var logger = host.Services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Starting data seeding...");

    using var scope = host.Services.CreateScope();

    var pimDb = scope.ServiceProvider.GetRequiredService<PimDbContext>();

    // Which server is about to be written to is worth stating plainly and on the console rather than
    // through the logger: AddUserSecrets above is applied last, so a connection string in user secrets
    // silently wins over one passed by environment variable or appsettings.
    if (pimDb.Database.IsSqlServer())
    {
        var target = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(pimDb.Database.GetConnectionString());
        Console.WriteLine($"Target: {target.DataSource} / {target.InitialCatalog}");
    }
    else
    {
        Console.WriteLine($"Target: {pimDb.Database.ProviderName}");
    }

    await pimDb.Database.EnsureCreatedAsync();
    // EnsureCreated leaves an existing database alone: add columns introduced since
    await pimDb.UpgradeSchemaAsync();

    if (pimDb.Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
    {
        var entityDbFunctions = new[]
        {
            PartyDbFunctions.CREATE_ALL,
            ProductDbFunctions.CREATE_ALL,
            FacetDbFunctions.CREATE_ALL,
        };
        foreach (var dbFunctions in entityDbFunctions)
        {
            foreach (var sql in dbFunctions)
            {
                try
                {
                    await pimDb.Database.ExecuteSqlRawAsync(sql);
                }
                catch
                {
                    // ignore
                }
            }
        }
    }

    if (useOpenFoodFacts)
    {
        var importer = scope.ServiceProvider.GetRequiredService<OpenFoodFactsImporter>();
        await importer.ImportAsync();
    }
    else
    {
        var seeder = scope.ServiceProvider.GetRequiredService<PimDataSeeder>();
        await seeder.SeedAsync();
    }

    logger.LogInformation("Data seeding completed.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    // The Serilog configuration can end up with no sinks, in which case the line above vanishes.
    // A seeding run that fails must not exit looking like it succeeded.
    Console.Error.WriteLine(ex);
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}