using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Regira.Office.Mail.MailGun;
using Serilog;
using Webshop.Admin.DependencyInjection;
using Webshop.Data;
using Webshop.DependencyInjection;
using Webshop.Identity.DependencyInjection;
using Webshop.WebshopDataGenerator.Infrastructure;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Configuration
        .AddUserSecrets(typeof(Program).Assembly, true);

    builder.Services
        .AddSerilog((sp, cfg) => cfg.ReadFrom.Configuration(builder.Configuration));

    builder.Services
        .AddAuthentication();
    builder.Services
        .AddWebshopAuthentication(builder.Configuration, _ => new MailGunMailer(builder.Configuration.GetSection("MailGun").Get<MailgunConfig>()!));

    builder.Services
        .AddWebshopServices(builder.Configuration)
        .AddAdminServices(builder.Configuration)
        .AddTransient<WebshopDataSeeder>();

    var host = builder.Build();

    var logger = host.Services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Starting data seeding...");

    using var scope = host.Services.CreateScope();

    var webshopDb = scope.ServiceProvider.GetRequiredService<WebshopDbContext>();
    await webshopDb.Database.EnsureCreatedAsync();
    if (!webshopDb.Articles.Any())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<WebshopDataSeeder>();
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