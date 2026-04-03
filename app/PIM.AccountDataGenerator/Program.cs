using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PIM.AccountDataGenerator.Infrastructure;
using PIM.Admin.DependencyInjection;
using PIM.Identity.Data;
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
        .AddSerilog((sp, cfg) => cfg.ReadFrom.Configuration(builder.Configuration));

    builder.Services
        .AddAuthentication();
    builder.Services
        .AddPimAuthentication(builder.Configuration, _ => new MailGunMailer(builder.Configuration.GetSection("MailGun").Get<MailgunConfig>()!));

    builder.Services
        .AddAdminServices(builder.Configuration)
        .AddTransient<AccountsDataSeeder>();

    var host = builder.Build();

    var logger = host.Services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Starting data seeding...");

    using var scope = host.Services.CreateScope();

    var accountsDb = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
    await accountsDb.Database.EnsureCreatedAsync();
    if (!accountsDb.Users.Any())
    {
        var accountsSeeder = scope.ServiceProvider.GetRequiredService<AccountsDataSeeder>();
        await accountsSeeder.SeedAsync();
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