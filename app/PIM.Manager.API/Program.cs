using Microsoft.AspNetCore.Authentication.JwtBearer;
using PIM.Admin.API.Infrastructure;
using PIM.Admin.DependencyInjection;
using PIM.Core.Constants;
using PIM.Data;
using PIM.DependencyInjection.Extensions;
using PIM.Identity.DependencyInjection;
using Regira.Entities.Web.DependencyInjection;
using Regira.Licensing.DependencyInjection;
using Regira.Office.Mail.MailGun;
using Regira.Security.Authentication.Web.OpenApi.Transformers;
using Scalar.AspNetCore;
using Serilog;

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
        });
    // Cycles, nulls, enum names and UTC request dates on both the MVC and Http.Json (OpenAPI) options,
    // plus the entity exception filter (EntityInputException → 400, constraint/concurrency → 409) for every action
    builder.Services.ConfigureDefaultJsonOptions(
        options => options.JsonSerializerOptions.AllowOutOfOrderMetadataProperties = true,
        options => options.SerializerOptions.AllowOutOfOrderMetadataProperties = true);

    // OpenAPI
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    });

    // Authentication (JWT)
    builder.Services
        .AddPimAuthentication(builder.Configuration, _ => new MailGunMailer(builder.Configuration.GetSection(PimConfig.MailGunSectionName).Get<MailgunConfig>()!))
        .WithJwtAuthentication();

    // Authorization policies
    builder.Services.AddAuthorization(options =>
    {
        // Any manager (Admin or Editor) may reach the API
        options.AddPolicy(PimPolicies.AdminOrEditor, policy =>
            policy.RequireClaim(PimClaimTypes.Permission, PimPermissionValues.Admin, PimPermissionValues.Editor));
        // Editor can manage all entities, except user management
        options.AddPolicy(PimPolicies.EditorOnly, policy =>
            policy.RequireClaim(PimClaimTypes.Permission, PimPermissionValues.Editor));
        // Admin can only manage users
        options.AddPolicy(PimPolicies.AdminOnly, policy =>
            policy.RequireClaim(PimClaimTypes.Permission, PimPermissionValues.Admin));
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
    // NOTE: AddAdminServices must be called BEFORE AddPimServices.
    // Both call UseMapsterMapping() which registers a TypeAdapterConfig singleton.
    // .NET DI resolves the LAST registered instance, so PIM's config (which contains
    // the custom MapWith rules for abstract types like Party->PartyDto) must win.
    builder.Services
        .UseRegira(builder.Configuration["Regira:LicenseKey"])
        .AddAdminServices(builder.Configuration)
        .AddPimServices(builder.Configuration, PimAppTypes.Manager);

    //builder.Services.AddSingleton(builder.Services);
    //builder.Services
    //    .AddFastEndpoints();

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
    // The database is created without migrations: add columns introduced since (e.g. ConcurrencyToken)
    using (var scope = app.Services.CreateScope())
        await scope.ServiceProvider.GetRequiredService<PimDbContext>().UpgradeSchemaAsync();

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
                .RequireAuthorization(PimPolicies.AdminOrEditor);
        })
        ;

    //app.MapFastEndpoints();
    //app.MapEntityEndpoints();

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