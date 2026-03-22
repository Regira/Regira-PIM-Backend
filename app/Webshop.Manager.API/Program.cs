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

    // Logging (Serilog)
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

    // Controllers & JSON
    builder.Services
        .AddControllers(options =>
        {
            options.Filters.Add<PermissionAuthorizationFilter>();
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

    // OpenAPI
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    });

    // Authentication (JWT)
    builder.Services
        .AddWebshopAuthentication(builder.Configuration, _ => new MailGunMailer(builder.Configuration.GetSection(WebshopConfig.MailGunSectionName).Get<MailgunConfig>()!))
        .WithJwtAuthentication();

    // Authorization policies
    builder.Services.AddAuthorization(options =>
    {
        // Any manager (Admin or Editor) may reach the API
        options.AddPolicy(WebshopPolicies.AdminOrEditor, policy =>
            policy.RequireClaim(WebshopClaimTypes.Permission, WebshopPermissionValues.Admin, WebshopPermissionValues.Editor));
        // Editor can manage all entities, except user management
        options.AddPolicy(WebshopPolicies.EditorOnly, policy =>
            policy.RequireClaim(WebshopClaimTypes.Permission, WebshopPermissionValues.Editor));
        // Admin can only manage users
        options.AddPolicy(WebshopPolicies.AdminOnly, policy =>
            policy.RequireClaim(WebshopClaimTypes.Permission, WebshopPermissionValues.Admin));
    });

    // CORS
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    // Entity services
    builder.Services
        .AddWebshopServices(builder.Configuration, WebshopAppTypes.Manager)
        .AddAdminServices(builder.Configuration);

    // APP configuration
    var app = builder.Build();

    // OpenAPI
    app.MapOpenApi()
        .AllowAnonymous();
    // Scalar API reference with JWT authentication
    app.MapScalarApiReference(options =>
    {
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecuritySchemes = [JwtBearerDefaults.AuthenticationScheme]
        };
    }).AllowAnonymous();

    await app.AddWalMode();

    app
        // HTTPS
        .UseHttpsRedirection()
        // CORS (must be before routing)
        .UseCors()
        // Request logging (Serilog)
        .UseSerilogRequestLogging()
        // Routing
        .UseRouting()
        // Authentication
        .UseAuthentication()
        // Authorization
        .UseAuthorization()
        // Endpoints
        .UseEndpoints(endpoints =>
        {
            endpoints.MapControllers()
                .RequireAuthorization(WebshopPolicies.AdminOrEditor);
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