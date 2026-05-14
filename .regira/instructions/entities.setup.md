# Regira Entities — Project Setup

> **AI Agent Rule**: Follow this guide to scaffold a new Regira Entities API project from scratch.
> Start from the **`BasicApi`** template in the shared `project.setup.md` guide and apply the Entities-specific additions below.
> In consumer repositories, prefer extracted `.github/instructions/regira/project.setup.md` when it exists locally. If it is not available yet, use the fallback baseline in this guide and keep the API surface aligned with `app.MapOpenApi()` plus `app.MapScalarApiReference()`.
> When available, combine with [`entities.namespaces.md`](./entities.namespaces.md) for exact `using` directives
> and [`entities.examples.md`](./entities.examples.md) for complete working code.

---

## Defaults

**Defaults (unless instructed otherwise):**
- **Database**: SQLite (`Microsoft.EntityFrameworkCore.Sqlite`)
- **Database initialization**: prefer `Database.EnsureCreated()` for the default SQLite starter/test setup; keep the local database disposable and do not scaffold an initial migration unless the user explicitly asks for migrations or chooses a more mature database
- **Mapping**: Mapster (`Regira.Entities.Mapping.Mapster`)
- **Project structure**: Per-entity folder structure
- **Service layer**: Default `EntityRepository` (unless complex logic requires wrapping)
- **Many-to-many relationships**: prefer option A

---

## Checklist

1. Create an ASP.NET Core Web API project — use the **`BasicApi`** template in the shared `project.setup.md` guide as the starting point.
2. Add `NuGet.Config` with the Regira feed — see the shared `project.setup.md` guide — **Shared Conventions → NuGet feed**.
3. Add required packages to `.csproj`.
4. Create `YourDbContext` deriving from `DbContext`.
5. Configure `Program.cs`.
6. Create the DI extension method (`AddEntityServices`).
7. Add entities — see [Entity Implementation Workflow](./entities.instructions.md#entity-implementation-workflow).

---

## NuGet Feed

> **→ See:** the shared `project.setup.md` guide — **Shared Conventions → NuGet feed** for the feed URL and `NuGet.Config` template.

---

## Packages

**Required:**
- `Regira.Entities.DependencyInjection`: Core entities framework with DI extensions
- Mapping:
    - `Regira.Entities.Mapping.Mapster`: Mapster integration (default)
    - OR `Regira.Entities.Mapping.AutoMapper`: AutoMapper integration

**Optional:** (when using entity attachments)
- `Regira.IO.Storage` — File storage (local file system or SFTP)
- `Regira.IO.Storage.Azure` — Azure Blob Storage support for attachments

---

## Step 1: Project Files

> Start from the **`BasicApi`** template in the shared `project.setup.md` guide and apply the Entities-specific additions below.
> If the shared guide is not available locally yet, use this minimum fallback baseline: ASP.NET Core Web API project, thin `Program.cs`, DI via extension methods, `app.MapOpenApi()`, `app.MapScalarApiReference()`, and no `UseSwaggerUI()`.

---

## Step 2: Create DbContext

> **→ See:** [`entities.examples.md`](./entities.examples.md) — §2 DbContext

- Derive from `DbContext`
- Call `modelBuilder.SetDecimalPrecisionConvention(18, 2)` in `OnModelCreating` for global decimal precision
- Add `DbSet<TEntity>` per entity incrementally
- Configure relationships per entity in `OnModelCreating`

---

## Step 3: Program.cs

**Changes to BasicApi**
```csharp
// ... usings from BasicApi
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Services;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;

// add JSON options to handle cycles and ignore nulls globally for all controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// add DbContext with interceptors for primers, normalizers, and auto-truncate
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    // use DB provider at wish
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
            .AddPrimerInterceptors(sp)
            .AddNormalizerInterceptors(sp)
            .AddAutoTruncateInterceptors());

// add entity services (repositories) and configurations
builder.Services.AddEntityServices();

// ...
// build app and configure as in BasicApi
```

> **Note:** `AddPrimerInterceptors(sp)` and `AddNormalizerInterceptors(sp)` require the `IServiceProvider` (`sp`) from the `AddDbContext` factory overload. Always use the `(sp, options) => ...` signature.

> **ValidateOnBuild:** Add the following to catch missing or mismatched entity service registrations at app startup rather than on the first request. A wrong generic parameter in `.For<>()` — or a `.For<>()` call that was never added — will throw immediately when the host builds instead of silently producing a confusing runtime error.
>
> ```csharp
> builder.Host.UseDefaultServiceProvider(o =>
> {
>     o.ValidateOnBuild = true;
>     o.ValidateScopes = true;
> });
> ```
>
> This validates constructor-injected dependencies only. Services resolved manually (e.g. via `GetRequiredEntityService<T>()` in controller extension methods) are not covered — those are caught by the improved error message in `ControllerExtensions.GetRequiredEntityService` instead.

> **OpenAPI/UI note:** If the shared project guide is not available locally yet, keep the API surface aligned with the Regira baseline here as well: use `app.MapOpenApi()` plus `app.MapScalarApiReference()` and do not add `Swashbuckle.AspNetCore` or `UseSwaggerUI()`.

> **SQLite starter note:** For the default SQLite starter/test setup, do not scaffold an initial EF migration. After `app = builder.Build()`, create a scope and call `Database.EnsureCreated()` instead:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}
```

> Treat the SQLite database as disposable test infrastructure. If the schema changes before the project adopts a mature database and explicit migrations, recreate the local SQLite database instead of adding migration files by default.

---

## Step 4: Create the DI Extension Method

Create `Extensions/ServiceCollectionExtensions.cs`.

> **→ See:** [`entities.examples.md`](./entities.examples.md) — §3 DI Extension Methods

- Call `options.UseDefaults()` to register primers, global query filters, and normalizer services
- Call `options.UseMapsterMapping()` (default) or `options.UseAutoMapper()` for DTO mapping
- Create one `Add{EntityNameInPlural}()` extension method per entity on `IEntityServiceCollection<TContext>` for composability
