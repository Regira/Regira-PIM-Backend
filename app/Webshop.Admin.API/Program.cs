using Regira.Office.Mail.MailGun;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json.Serialization;
using Webshop.Admin.DependencyInjection;
using Webshop.DependencyInjection;
using Webshop.Identity.Data;
using Webshop.Identity.DependencyInjection;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    builder.Services.AddOpenApi();

    builder.Services
        .AddWebshopAuthentication(o =>
        {
            var config = builder.Configuration;
            var options = config.GetSection("Identity").Get<WebshopIdentityOptions>()!;
            o.SecretKey = options.SecretKey;
            o.Audiences.AddRange(options.Audiences);
            o.AddMailer(_ => new MailGunMailer(config.GetSection("MailGun").Get<MailgunConfig>()!));
        });
    builder.Services
        .AddWebshopServices(builder.Configuration);
    builder.Services
        .AddAdminServices(builder.Configuration);

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WebshopAccountsDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}