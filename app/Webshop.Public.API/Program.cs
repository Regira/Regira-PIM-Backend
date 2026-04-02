using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Regira.Office.Mail.MailGun;
using Regira.Security.Authentication.Web.OpenApi.Transformers;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json.Serialization;
using Webshop.Core.Constants;
using Webshop.DependencyInjection;
using Webshop.Identity.Data;
using Webshop.Identity.DependencyInjection;
using Webshop.Public.API.Infrastructure;

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
            options.Filters.Add<WriteAuthorizationFilter>();
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
        // Customer can only view and manage their own orders
        options.AddPolicy(WebshopPolicies.CustomerOnly, policy =>
            policy.RequireClaim(WebshopClaimTypes.Permission, WebshopPermissionValues.Customer));
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

    // Entity services + DbContexts
    builder.Services
        .AddDbContext<AccountsDbContext>(options =>
        {
            var sqlServerConnectionString = builder.Configuration["ConnectionStrings:SqlServer:Accounts"];
            var sqliteConnectionString = builder.Configuration["ConnectionStrings:Sqlite:Accounts"];

            _ = !string.IsNullOrWhiteSpace(sqlServerConnectionString)
                ? options.UseSqlServer(sqlServerConnectionString, db => db.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                : options.UseSqlite(sqliteConnectionString, db => db.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        })
        .AddWebshopServices(builder.Configuration, WebshopAppTypes.Public);

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
        .UseEndpoints(endpoints =>
        {
            endpoints.MapControllers()
                .RequireAuthorization(WebshopPolicies.CustomerOnly);
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
