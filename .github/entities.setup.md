# Regira Entities — Project Setup

> **AI Agent Rule**: Follow this guide to scaffold a new Regira Entities API project from scratch.
> Start from the **`BasicApi`** template in [`project.setup.md`](./project.setup.md) and apply the Entities-specific additions below.
> Always combine with [`entities.namespaces.md`](./entities.namespaces.md) for exact `using` directives
> and [`entities.examples.md`](./entities.examples.md) for complete working code.

---

## Defaults

**Defaults (unless instructed otherwise):**
- **Database**: SQLite (`Microsoft.EntityFrameworkCore.Sqlite`)
- **Mapping**: Mapster (`Regira.Entities.Mapping.Mapster`)
- **Project structure**: Per-entity folder structure
- **Service layer**: Default `EntityRepository` (unless complex logic requires wrapping)
- **Many-to-many relationships**: prefer option A

---

## Checklist

1. Create an ASP.NET Core Web API project — use the **`BasicApi`** template in [`project.setup.md`](./project.setup.md) as the starting point.
2. Add `NuGet.Config` with the Regira feed — see [`project.setup.md`](./project.setup.md) — **Shared Conventions → NuGet feed**.
3. Add required packages to `.csproj`.
4. Create `YourDbContext` deriving from `DbContext`.
5. Configure `Program.cs`.
6. Create the DI extension method (`AddEntityServices`).
7. Add entities — see [Entity Implementation Workflow](./entities.instructions.md#entity-implementation-workflow).

---

## NuGet Feed

> **→ See:** [`project.setup.md`](./project.setup.md) — **Shared Conventions → NuGet feed** for the feed URL and `NuGet.Config` template.

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

> Start from the **`BasicApi`** template in [`project.setup.md`](./project.setup.md) and apply the Entities-specific additions below.

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

---

## Step 4: Create the DI Extension Method

Create `Extensions/ServiceCollectionExtensions.cs`.

> **→ See:** [`entities.examples.md`](./entities.examples.md) — §3 DI Extension Methods

- Call `options.UseDefaults()` to register primers, global query filters, and normalizer services
- Call `options.UseMapsterMapping()` (default) or `options.UseAutoMapper()` for DTO mapping
- Create one `Add{EntityNameInPlural}()` extension method per entity on `IEntityServiceCollection<TContext>` for composability
