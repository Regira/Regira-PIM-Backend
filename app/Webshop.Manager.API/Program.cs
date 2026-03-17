using Microsoft.AspNetCore.Authentication.JwtBearer;
using Regira.Office.Mail.MailGun;
using Regira.Security.Authentication.Web.OpenApi.Transformers;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json.Serialization;
using Webshop.Admin.DependencyInjection;
using Webshop.Core.Constants;
using Webshop.DependencyInjection;
using Webshop.Identity.DependencyInjection;
using Webshop.Manager.API.Infrastructure;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddControllers(options =>
        {
            options.Filters.Add<WriteAuthorizationFilter>();
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    });

    builder.Services
        .AddWebshopAuthentication(builder.Configuration, _ => new MailGunMailer(builder.Configuration.GetSection(WebshopConfig.MailGunSectionName).Get<MailgunConfig>()!))
        .WithJwtAuthentication();

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(WebshopPolicies.EditorOnly, policy =>
            policy.RequireClaim(WebshopClaimTypes.Permission, WebshopPermissionValues.Editor));
        options.AddPolicy(WebshopPolicies.AdminOnly, policy =>
            policy.RequireClaim(WebshopClaimTypes.Permission, WebshopPermissionValues.Admin));
    });

    builder.Services
        .AddWebshopServices(builder.Configuration, true)
        .AddAdminServices(builder.Configuration);

    var app = builder.Build();

    app.MapOpenApi()
        .AllowAnonymous();
    app.MapScalarApiReference(options =>
    {
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecuritySchemes = [JwtBearerDefaults.AuthenticationScheme]
        };
    });
    app.UseHttpsRedirection();
    app
        .UseRouting()
        .UseAuthentication()
        .UseAuthorization()
        .UseEndpoints(endpoints =>
        {
            endpoints.MapControllers()
                .RequireAuthorization();
        });
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