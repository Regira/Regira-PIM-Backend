# Copilot Instructions — Project Templates

> **NuGet package versions:** Always resolve to the **latest stable version** of every package unless the user explicitly requests a specific version. The version numbers shown throughout this document are illustrative only.

This solution contains some reusable starter templates.
Identify the user's use case and apply the matching template below as the starting point.

---

## Template Selection Guide

| Requirement                                          | Template                    |
|------------------------------------------------------|-----------------------------|
| Script, batch job, or CLI utility                    | `ConsoleWithLogging`        |
| Standard hosted API, Minimal API and Controllers, no auth  | `BasicApi`            |
| Lightweight self-hosted internal API, no auth        | `SelfHostingApi`     |
| API protected by API key and/or JWT Bearer           | `SelfHostingApiWithAuth`    |
| Must be deployable as a Windows Service              | `SelfHostingApi`     |
| Controller-based routing with enforced authorization | `SelfHostingApiWithAuth`    |
| Minimal API endpoints with authentication            | `SelfHostingApiWithAuth`    |

---

## Shared Conventions

These rules apply to all templates.

### Framework & language

- **TFM:** `net10.0` — do not change unless explicitly asked.
- **C# 14** — use modern features (primary constructors, collection expressions `[..]`, raw string literals `"""`, file-scoped namespaces) where appropriate.
- `<ImplicitUsings>enable</ImplicitUsings>` and `<Nullable>enable</Nullable>` are always on.
- Replace `MyProject` with the actual project name throughout all files.

### Basic Project file

**Console App**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

**API**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

### Default File structure

```
MyProject/
├── Properties/
│   └── launchSettings.json // API only
├── Infrastructure/
│   └── HostingExtensions.cs   # See Shared Conventions
├── Program.cs
├── appsettings.json
```

### NuGet feed

`Regira.*` packages are served from a private feed. Add `NuGet.Config` to the solution root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
    <add key="Regira" value="https://packages.regira.com/v3/index.json" />
  </packageSources>
</configuration>
```

### Logging (serilog)

All templates use Serilog with console + rolling file sinks configured from `appsettings.json`.

**Packages**

```xml
<PackageReference Include="Microsoft.Extensions.Configuration.UserSecrets" Version="*" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="*" />
    <PackageReference Include="Serilog.Extensions.Hosting" Version="*" />
    <PackageReference Include="Serilog.Settings.Configuration" Version="*" />
    <PackageReference Include="Serilog.Sinks.Console" Version="*" />
    <PackageReference Include="Serilog.Sinks.File" Version="*" />
```

**Packages — Web APIs **

```xml
<PackageReference Include="Serilog.AspNetCore" Version="*" />
```

**Bootstrap pattern (web APIs)**

Wrap the entire `Program.cs` body in a bootstrap logger + try/catch/finally:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host
        .UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

    // ... rest of setup ...

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
```

**`appsettings.json` Serilog block**

Used by all templates. Web APIs use `"Default": "Information"`; `ConsoleWithLogging` uses `"Default": "Debug"`.

```json
"Serilog": {
  "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft.AspNetCore": "Warning",
      "System": "Warning"
    }
  },
  "WriteTo": [
    { "Name": "Console" },
    {
      "Name": "File",
      "Args": {
        "path": "logs/MyProject-.log",
        "restrictedToMinimumLevel": "Information",
        "rollingInterval": "Month",
        "rollOnFileSizeLimit": true,
        "retainedFileCountLimit": 12
      }
    }
  ],
  "Enrich": [ "FromLogContext" ]
}
```

### OpenAPI & Scalar UI

All API templates expose:
- **OpenAPI JSON** `/openapi/v1.json` — `app.MapOpenApi()`
- **Scalar UI** `/scalar` — `app.MapScalarApiReference()`

**Nuget packages — Web API**
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="*" />
<PackageReference Include="Scalar.AspNetCore" Version="*" />
```

### Launch API

All API templates include this launch profile, opening the Scalar UI on start.

**Properties/launchSettings.json**
```json
{
  "profiles": {
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "scalar",
      "applicationUrl": "<URLS>",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### Extension methods

- Services and middleware registration are placed in extension methods, never inline in `Program.cs`.
  - Console/Worker: `IHostBuilder` extensions in `Infrastructure/HostExtensions.cs`
  - Web APIs: per-concern static extension classes (e.g. `Infrastructure/HostExtensions.cs` or `Infrastructure/EndpointExtensions.cs`)
- `Program.cs` stays as a thin orchestrator: build → configure → run.

### Console configuration

Enrich console apps with appsettings, user secrets and environment variables.
(Not applicable to APIs since they implement user secrets by default.)

**.csproj file — Console App**
```xml
  <!-- NuGet package -->
  <PackageReference Include="Microsoft.Extensions.Configuration.UserSecrets" Version="*" />

  <!-- include appsettings.json in output for console apps -->
  <ItemGroup>
    <Content Include="appsettings.json">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
```

**HostExtensions**
```csharp
public static IHostBuilder AddConfiguration(this IHostBuilder builder)
{
    return builder.ConfigureAppConfiguration((_, config) =>
    {
        config.Sources.Clear();
        // add configuration
        config
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", true, true)
#if DEBUG
            .AddUserSecrets(typeof(Program).Assembly, true)
#endif
            ;
    });
}
```

### Windows Service support

- `WindowsServiceHelpers.IsWindowsService()` guards `UseWindowsService()` so the app runs normally outside the service host.
- `AddWindowsServiceInstaller` generates `install.bat` / `uninstall.bat` on first run (idempotent).
- `AddWindowsServiceInstaller` is provided by `Regira.System.Hosting` (`WindowsServiceHostExtensions`).

### Authentication conventions

- API keys are passed in the `X-Api-Key` request header.
- JWT tokens use the `Authorization: Bearer <token>` header.
- The `"Smart"` `PolicyScheme` auto-selects the correct scheme based on the header present.
- Authorization is **enforced globally** via `.RequireAuthorization()` on `MapControllers()`.
- OpenAPI / Scalar endpoints are explicitly marked `.AllowAnonymous()`.
- Controllers that must be public use `[AllowAnonymous]` per action.

**Nuget packages**
```xml
<PackageReference Include="Regira.Security.Authentication" Version="*" />
<PackageReference Include="Regira.Security.Authentication.Web" Version="*" />
```

**appsettings.json — Authentication block**
```json
{
  "Authentication": {
    "ApiKeys": [
      { "OwnerId": "MyUser", "Key": "REPLACE-WITH-GUID" }
    ],
    "Jwt": {
      "Secret": "REPLACE-WITH-RANDOM-SECRET-MIN-32-CHARS-LONG!!"
    }
  }
}
```

**Key auth model types**

Provided by `Regira.Security.Authentication`:

- **`ApiKeyOwner`** — `{ OwnerId, Key }` — registered caller identity
- **`ApiKeyDefaults`** — `AuthenticationScheme` + `HeaderName` constants
- **`JwtTokenOptions`** — `{ Secret, Authority, Audience, LifeSpan, ... }` — JWT configuration

### Self hosting APIs

- Kestrel is always configured from `appsettings.json` via `options.Configure(context.Configuration.GetSection("Kestrel"))`.

**Nuget packages**
```xml
<PackageReference Include="Regira.System.Hosting" Version="*" />
```

**appsettings.json — Kestrel block**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:9001"
      },
      "Https": {
        "Url": "https://localhost:9002",
        "Certificate": {
          "Path": "xxx.pfx", // Better include in secrets or safer sources
          "Password": "XXX", // Better include in secrets or safer sources
        }
      }
    }
  }
}
```

- User secrets are only added in `#if DEBUG` blocks.

### Self-signed certificate (`your-certificate.pfx`)

Templates 3 and 4 require a certificate for local HTTPS. Generate one with OpenSSL:

```sh
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365
openssl pkcs12 -export -out your-certificate.pfx -inkey key.pem -in cert.pem
```

---

## Template 1 — `ConsoleWithLogging`

### Use when
Standalone console application for a task, script, or batch job — with structured logging, dependency injection, and configuration support.

### Project file

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <Content Include="appsettings.json">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
</Project>
```

### `Program.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyProject.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = Host.CreateDefaultBuilder(args);
    var host = builder
        .AddSerilog()
        .AddConfiguration()
        .AddServices()
        .Build();

    using var scope = host.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Start");
    Console.WriteLine();

    // Execute code here

    Console.WriteLine();
    logger.LogInformation("Finished");
}
catch (Exception ex)
{
    Log.Error(ex, "Host failed");
}
finally
{
    Console.WriteLine("Press enter to exit");
    Console.ReadLine();
    Log.CloseAndFlush();
}
```

### `Infrastructure/HostingExtensions.cs`

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MyProject.Infrastructure;

public static class HostingExtensions
{
    public static IHostBuilder AddConfiguration(this IHostBuilder builder)
        => builder.ConfigureAppConfiguration((_, config) =>
        {
            config.Sources.Clear();
            config
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", true, true)
#if DEBUG
                .AddUserSecrets(typeof(Program).Assembly, true)
#endif
                ;
        });

    public static IHostBuilder AddServices(this IHostBuilder builder)
        => builder.ConfigureServices((context, services) =>
        {
            var config = context.Configuration;
            // register services here
        });

    public static IHostBuilder AddSerilog(this IHostBuilder builder)
    {
        builder.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));
        return builder;
    }
}
```

---

## Template 2 — `BasicApi`

### Use when
Standard ASP.NET Core API hosted on IIS, Azure, or Docker. No authentication. Supports both controller-based routing and Minimal API endpoints.

### `Program.cs`

```csharp
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // recommended
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; // recommended
            // options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // enable at wish
        });
    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.MapOpenApi();
    app.MapScalarApiReference();
    
    app.UseHttpsRedirection();

    // app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    // app.AddEndpoints(); // enable at wish

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
```

---

## Template 3 — `SelfHostingApi`

### Use when
Lightweight self-hosted HTTP API, optionally deployable as a Windows Service. No authentication.

### `Program.cs`

```csharp
using Microsoft.Extensions.Hosting.WindowsServices;
using Regira.System.Hosting.WindowsService;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

    if (WindowsServiceHelpers.IsWindowsService())
        builder.Host.UseWindowsService();

    builder.WebHost.ConfigureKestrel((context, options) =>
        options.Configure(context.Configuration.GetSection("Kestrel")));

    builder.Services.AddControllers();

    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.MapOpenApi();
    app.MapScalarApiReference();

    app
        .UseRouting()
        //.UseAuthentication()
        .UseAuthorization()
        .UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

    var host = app.Services.GetRequiredService<IHost>();
    host.AddWindowsServiceInstaller(new WindowsServiceOptions { ServiceName = "MyProject" }); // Replace with actual service name

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
```
---

## Template 4 — `SelfHostingApiWithAuth`

### Use when
Self-hosted HTTP API with controller-based routing and access control via API key and/or JWT Bearer tokens.

### `Program.cs`

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Regira.Security.Authentication.ApiKey.Extensions;
using Regira.Security.Authentication.ApiKey.Models;
using Regira.Security.Authentication.Jwt.Extensions;
using Regira.Security.Authentication.Web.OpenApi.Transformers;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

    builder.WebHost.ConfigureKestrel((context, options) =>
        options.Configure(context.Configuration.GetSection("Kestrel")));

    builder.Services.AddControllers();

    // Authentication — API Key
    var authBuilder = builder.Services
        .AddApiKeyAuthentication()
        .AddInMemoryApiKeyAuthentication(
            builder.Configuration.GetSection("Authentication:ApiKeys").ToApiKeyOwners()
        );

    // Authentication — JWT Bearer
    builder.Services.AddJwtAuthentication(o =>
    {
        o.Secret = builder.Configuration["Authentication:Jwt:Secret"]
            ?? throw new NullReferenceException("Secret is missing");
    });

    // Policy scheme: auto-forward to JWT when Authorization: Bearer header is present
    authBuilder.AddPolicyScheme("Smart", "Authorization Bearer or ApiKey", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            return authHeader?.StartsWith("Bearer ") == true
                ? JwtBearerDefaults.AuthenticationScheme
                : ApiKeyDefaults.AuthenticationScheme;
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(
                ApiKeyDefaults.AuthenticationScheme,
                JwtBearerDefaults.AuthenticationScheme)
            .Build();
    });

    // OpenAPI
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer<ApiKeySecurityDocumentTransformer>();
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    });

    var app = builder.Build();

    app.MapOpenApi()
        .AllowAnonymous();
    app.MapScalarApiReference(options =>
    {
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecuritySchemes = [
                ApiKeyDefaults.AuthenticationScheme,
                JwtBearerDefaults.AuthenticationScheme
            ]
        };
    });

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
```

