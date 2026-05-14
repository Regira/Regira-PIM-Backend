# Regira Entities AI Agent Instructions

> A generic, extensible framework for managing data entities in .NET with standardized CRUD operations, filtering, sorting, and includes.

## Projects

| Project | Package | Purpose |
|---------|---------|----------|
| `Common.Entities` | `Regira.Entities` | Shared abstractions and interfaces |
| `Entities.EFcore` | `Regira.Entities.EFcore` | EF Core `EntityRepository` |
| `Entities.Web` | `Regira.Entities.Web` | ASP.NET Core `EntityControllerBase` |
| `Entities.Web.FastEndpoints` | `Regira.Entities.Web.FastEndpoints` | `MapEntityEndpoints()` auto-registration (preferred) |
| `Entities.DependencyInjection` | `Regira.Entities.DependencyInjection` | `UseEntities()` / `.For<>()` DI builder |
| `Entities.Mapping.AutoMapper` | `Regira.Entities.Mapping.AutoMapper` | AutoMapper integration |
| `Entities.Mapping.Mapster` | `Regira.Entities.Mapping.Mapster` | Mapster integration |

Always prefer clear, conventional patterns over clever solutions. Default to the more feature-rich options when in doubt.

---

## Quick Agent Playbook

Use this as the primary checklist.

### Create a New Regira API Project

1. Create an ASP.NET Core Web API project targeting the latest .NET version (or the solution's target).
2. Add `NuGet.Config` with the Regira feed (see below).
3. Add the required Regira and EF Core packages to the project file.
4. Create a `YourDbContext` deriving from `DbContext` and configure it.
5. In `Program.cs`:
   - Register `YourDbContext` via `AddDbContext<YourDbContext>(...)`
   - Inside the DbContext configuration, add any interceptors (primers, normalizers, auto-truncate) as needed.
   - Call `UseEntities<YourDbContext>(...)` on `builder.Services`, preferably via an extension method.
   - Inside `UseEntities` config, call `.UseDefaults()` by default, then add mapping and any global services.
   - `UseEntities()` returns a `IEntityServiceCollection` to configure the entities using `.For()`, preferably via extension methods per main Entity.
   - Add `builder.Services.AddFastEndpoints()` and call `app.UseFastEndpoints()` + `app.MapEntityEndpoints()` to auto-register all CRUD routes (preferred). For controller-based routing, use standard ASP.NET controller setup instead.
6. Add entities using the workflow below.

### Add a New Entity to an Existing Project

1. Add the entity class and implement the appropriate interfaces.
2. Add `SearchObject`, `SortBy`, `Includes`, and DTOs as needed.
3. Add optional query builder / processor / prepper classes.
4. Register the entity on the `EntityServiceCollection` using `.For<TEntity,...>(...)`.
5. Prefer `app.MapEntityEndpoints()` for zero-boilerplate CRUD route registration — no per-entity step needed. Add an `EntityControllerBase` controller only when DTO mapping, custom auth, or advanced sort/includes filtering is required.
6. Add `DbSet<TEntity>` to `YourDbContext` and configure relationships.
7. If the project still uses the default SQLite starter database, keep it migration-free and rely on `Database.EnsureCreated()` with a disposable local database.
8. Only create and apply EF migrations when the user explicitly wants migration-based schema management or has moved to a more mature database provider.

### Modify an Existing Entity

1. Update the entity class and related `DbSet`/relationships.
2. Update DTOs and mapping configuration.
3. Update `SearchObject`, enums, and query builders if filters/sorting change.
4. Adjust processors, preppers, and normalizers if behavior changes.
5. If the project still uses the default SQLite starter database, keep the model aligned and recreate the local test database when schema changes require `EnsureCreated()` to rebuild it.
6. Create and apply a migration only when the user explicitly wants migration-based schema management or the project has moved beyond the disposable SQLite starter setup.

---

## Namespace Reference

> **→ See:** [`entities.namespaces.md`](./entities.namespaces.md)

- You are **NOT** allowed to guess, invent, or assume any namespace.
- **Always use exact namespaces — never guess.**  

---

## Signatures Reference

> **→ See:** [`entities.signatures.md`](./entities.signatures.md)

- Use this file to look up the **exact signatures** of all interfaces, classes, and extension methods in the framework.
- **Do not guess method names, parameter types, or return types** — always verify against this reference.
- Covers: entity interfaces, service interfaces, controller base classes, service builders, mapping types, response types, and more.

---

## Core Understanding

### Framework Architecture

**Regira Entities** is a generic, extensible framework for managing data entities in .NET with standardized CRUD operations.

**Key Components:**
- **Entity Models**: POCO classes implementing `IEntity<TKey>`
- **Services**: `IEntityService` (default: `EntityRepository` backed by DbContext)
- **Controllers**: API endpoints inheriting from `EntityControllerBase`
- **DTOs**: Separate read (`TDto`) and write (`TInputDto`) models
- **Pipeline Services**: QueryBuilders, Processors, Preppers, Primers, AfterMappers

### Generic Type System

| Type | Required | Purpose | Example |
|------|----------|---------|---------|
| `TEntity` | ✓ | The entity class | `Product` |
| `TKey` | ✓* | Primary key type (*default: `int`) | `Guid`, `int` |
| `TSearchObject` | ○ | Advanced filtering | `ProductSearchObject` |
| `TSortBy` | ○ | Sorting enum | `ProductSortBy` |
| `TInclude` | ○ | Navigation properties enum | `ProductIncludes` |
| `TDto` | ○ | Read/display model | `ProductDto` |
| `TInputDto` | ○ | Create/update model | `ProductInputDto` |

### Processing Pipelines

**Read Pipeline:**
```
EntitySet → QueryBuilders (Filters → Sorting → Paging → Includes) → Processors → Mapping → AfterMapping*
```
*AfterMapping is only executed in API controllers and FastEndpoints endpoints

**Write Pipeline:**
```
Input → Mapping* → AfterInput* → Preppers → SaveChanges → Primers (Interceptors) → Submit
```
*Only executed in API controllers

**⚠️ Important**: `SaveChanges()` must be called explicitly to persist changes, similar to standard EF Core behavior.
Base controllers call `SaveChanges()` automatically, but when using `IEntityService` directly, you must call it yourself.

---

## Decision-Making Guidelines

### When to Use Inline vs Separate Classes

**Use INLINE configuration when:**
- Simple logic (< 10 lines)
- Entity-specific, not reusable
- Rapid prototyping
- Testing whole EntityService is sufficient (no need to test Helper Services separately)

**Use SEPARATE classes when:**
- Complex logic
- Needs dependency injection (DbContext, services)
- Reusable across entities
- Production-quality code requiring testability
- Testing complex logic in isolation is beneficial

### Web Endpoints

**FastEndpoints (preferred — no per-entity controller boilerplate):**
- Call `app.MapEntityEndpoints()` in `Program.cs` — auto-registers CRUD routes for every `IEntityService<,>` in DI
- No per-entity code needed; routes follow `api/{entityname}s` convention
- Use `EntityAutoEndpointsOptions` to override routes for fully custom paths (irregular plurals are handled automatically)
- Note: auto-registration uses raw entities (no DTOs); use endpoint base classes for DTO-aware endpoints

**Use `EntityControllerBase` when:**
- The project already uses MVC controllers and consistency is preferred
- Per-endpoint or per-controller customisation is more natural in the MVC attribute model
- You don't want an extra dependency on FastEndpoints (though it's a very lightweight dependency)

```csharp
// Minimal (no search or sorting)
EntityControllerBase<TEntity, TDto, TInputDto>

// With search
EntityControllerBase<TEntity, TSearchObject, TDto, TInputDto>
EntityControllerBase<TEntity, TKey, TSearchObject, TDto, TInputDto>

// Full-featured (recommended for complex scenarios)
EntityControllerBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>
```

- Don't expose entity classes directly in API responses — prefer DTOs
- Extend `SearchObject` to add filtering rather than creating extra endpoints
- Add custom controller actions only when base methods are insufficient

### Service Layer Decisions

**Use default `EntityRepository` when:**
- Standard CRUD is sufficient
- Custom logic fits in QueryBuilders / Processors / Preppers / Primers

**Create custom wrapping service when:**
- Caching layer is needed
- Security/authorization logic needed at service level
- Auditing or logging around operations
- Multiple data sources need to be combined
- Complex validation spanning multiple entities

---

## Project Creation Workflow

Don't copy the example code, use it as a reference and follow the steps to create your own implementation.

### Step 1: Project Files

- Use latest .NET TargetFramework in the project file (currently .net10)
- Add NuGet package(s)
- Apply template files (*.csproj, appsettings.json, Program.cs)

**Defaults (unless instructed otherwise):**
- **Database**: SQLite (`Microsoft.EntityFrameworkCore.Sqlite`)
- **Database initialization**: for the default SQLite starter/test setup, prefer `Database.EnsureCreated()` over scaffolding an initial migration
- **Mapping**: Mapster (`Regira.Entities.Mapping.Mapster`)
- **Project structure**: Per-entity folder structure
- **Service layer**: Default `EntityRepository` (unless complex logic requires wrapping)

> **→ See:** [`entities.setup.md`](./entities.setup.md)

### Step 2: Create DbContext

> **→ See:** [`entities.examples.md`](./entities.examples.md) — DbContext

### Step 3: Create the DI Extension Method

- Enable Regira Entities by `UseEntities()` which returns an `IEntityServiceCollection` for configuring entities
- Use extension methods per entity using `.For()` for clean, composable DI registration
- Use inline config for simple logic; separate classes for complex logic or when DI is needed

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Setup

---

## Entity Implementation Workflow

Use the table below to decide which extension points are actually needed before adding more classes.

| Step | Default | Add it when |
|------|---------|--------------|
| 7. Processors | Skip by default | You need to fill `[NotMapped]` or other derived values after fetching from the database |
| 8. Preppers | Skip by default | You must adjust entities before `SaveChanges()`, manage child collections, or set totals, codes, and foreign keys |
| 9. Primers | Skip by default | You need EF Core interceptor behavior during `SaveChanges()` or transaction-aware stamping across modified entities |
| 10. Mapping & AfterMappers | Skip extra mapping config by default | DTO shape diverges from the entity, nested related mappings need `AddMapping<TSource, TTarget>()`, or DTO enrichment requires custom logic or DI |

Mnemonic: Preppers prepare entity state before `SaveChanges()`; Primers prepare entity state during `SaveChanges()` via EF Core interceptors.

### Step 1: Create Entity Model

- Keep entities as POCOs — data only, no business logic
- Use data annotations (`[Required]`, `[MaxLength]`, `[Range]`) directly on entity properties
- Use `SetDecimalPrecisionConvention` in DbContext instead of setting precision per property
- Nullable: follow interfaces when type is nullable, nullable properties can be combined with [Required] annotation

**Interface selection checklist:**

| Interface | Add when… |
|-----------|-----------|
| `IEntityWithSerial` | int primary key (auto-increment). Shortcut for `IEntity<int>` |
| `IEntity<TKey>` | Non-int primary key (e.g. `Guid`) |
| `IHasTimestamps` | Track Created + LastModified |
| `IArchivable` | Soft-delete instead of hard-delete |
| `IHasTitle` | Entity has a short display name |
| `IHasDescription` | Entity has a long text field |
| `IHasCode` | Entity has a short unique code |
| `ISortable` | Used as a sortable child collection |
| `IHasNormalizedContent` | Entity uses normalized text for search |
| `IHasAttachments` | Entity can have file attachments |

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Category entity

### Step 2: Create SearchObject

- Inherit from `SearchObject` and add filter properties as needed. 
- When no custom search object is implemented, the default `SearchObject` is used as fallback.
- Prefer using `ICollection<TKey>` for FK filters to allow multiple values.

```csharp
public class SearchObject : SearchObject<int>;
public class SearchObject<TKey> : ISearchObject<TKey>
{
    public TKey? Id { get; set; }
    public ICollection<TKey>? Ids { get; set; }
    public ICollection<TKey>? Exclude { get; set; }
    public string? Q { get; set; }

    public DateTime? MinCreated { get; set; }
    public DateTime? MaxCreated { get; set; }
    public DateTime? MinLastModified { get; set; }
    public DateTime? MaxLastModified { get; set; }

    public bool? IsArchived { get; set; }
}
```

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Category entity

### Step 3: Create SortBy Enum

- `SortBy` is a plain (non-`[Flags]`) enum
- Multiple sortBy values can be passed in an array and will be applied in order
- The framework falls back to `EntitySortBy` if no custom sorting options implemented

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Product entity

### Step 4: Create Includes Enum

> `Includes` is a `[Flags]` enum — values are combined with bitwise OR.
> The framework falls back to `EntityIncludes` if no custom includes options implemented.
> Base `EntityIncludes` is intentionally minimal (`Default`, `All`). Use it when the entity only needs the base "no extra relations" versus "include all configured relations" behavior.
> If the entity needs named domain-specific flags such as `Categories`, `Parents`, or `Children`, define a custom `[Flags]` enum and use that same `TIncludes` type consistently in `.For<>()`, `EntityControllerBase<>`, processors, and any direct `IEntityService<>` injections.
> List function should not return navigation properties by default — they must be explicitly requested via the `includes` parameter to prevent over-fetching and performance issues.
> Details function can return all navigation properties by default since it's for a single item and the consumer is likely to want the related data.

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Category entity

### Step 5: Create DTOs

- Include `Id` in `InputDto` to support the Save (upsert) action
- Exclude normalized fields from DTOs — they are for internal use only
- Exclude auto-generated fields (`Created`, `LastModified`, `NormalizedContent`) from `InputDto`
- Exclude secured fields (e.g. `Password`) from DTOs
- When using Attachments, exclude full File paths, since the FileService accepts relative paths (identifiers)
- Try to facilitate mapping by keeping DTO structure similar to the entity (e.g. nested related entities instead of flattening)
- Use navigation properties in DTOs instead of flattening related entity data: this preserves structure and enables richer client-side handling (e.g. avoid `CategoryTitle`, but use `Category`=>`Title`)
- Only include child collections in `InputDto` when they are configured with `e.Related(...)`
- Use `AfterMapper` for computed/calculated properties (e.g. URLs, display names) in DTO

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Category entity

### Step 6: Create Query Builders

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Product entity

**Option A: Inline** — use for simple logic (< 10 lines), entity-specific, no DI needed.  
**Option B: Separate class** — use when complex logic, DI, or reuse is required.

**⚠️ Important:** Never reference `[NotMapped]` or processor-populated properties inside `.Filter(...)`, `.SortBy(...)`, or any query lambda that must be translated to SQL. Those values do not exist in the database query. If a filter needs derived data, move it into a separate query builder with DI and use database-backed joins or subqueries.

### Step 7: Processors (Optional)

Use processors to fill `[NotMapped]` properties or enrich entities **after** fetching from the database.

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Category entity (CategoryProcessor) / Additional Patterns > Inline processor

### Step 8: Preppers (Optional)

- Before saving
- manage child entities (if not using `e.Related()` in entity configuration)
- (re)calculate totals, generate codes, set FKs, etc.

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Additional Patterns > Prepper

**Variants:** inline (simple), inline with original (create vs update), inline with DbContext, separate class, `e.Related(x => x.ChildCollection)` shortcut.

### Step 9: Primers (Optional)

- Run during `SaveChanges()` via EF Core interceptors
- Allow checking other modified entities, ready to be persisted in the same transaction
- Require use of `AddPrimerInterceptors(sp)` in `AddDbContext()`

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Additional Patterns > Primers

### Step 10: Mapping & AfterMappers (Optional extra configuration)

- Skip mapping when using Mapster and DTO structure is similar to the entity and Mapster's conventions can handle it automatically
- Use `.After(...)` to enrich the DTO after `Entity→DTO` mapping (computed properties, URLs)
- Use `.AfterInput(...)` to modify the entity after `InputDto→Entity` mapping
- `AddMapping<TSource, TTarget>()` is required for some non-obvious nested mappings:
  - Same-type registration such as `e.AddMapping<ProductCategoryDto, ProductCategoryDto>()` enables nested output DTO projection
  - Cross-type registration such as `e.AddMapping<ProductCategoryInputDto, ProductCategory>()` enables child `InputDto` collections used with `e.Related(...)`
- Use `.After<TAfterMapper>()` for a separate class when DI is needed
- Use `options.AddAfterMapper<T>()` to register a global AfterMapper

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Additional Patterns > AddMapping / AfterMapper

### Step 11: Configure Entities

Use `.For<TEntity, ...>(...)` to register each entity and configure its services. The generic type arguments determine which features are enabled and which base controller to use.
Child entities configured with `e.Related()` don't need their own `.For<>()` registration.

Before writing the controller, verify the `.For<>()` registration and controller pairing in [`entities.signatures.md`](./entities.signatures.md). The controller must mirror the registration generics exactly.

> **→ See:** [`entities.examples.md`](./entities.examples.md) (All entities)

### Step 12: Configure Web Endpoints

**Option A: FastEndpoints auto-registration (preferred)**

No per-entity step needed. `app.MapEntityEndpoints()` (called once in `Program.cs`) automatically covers every entity registered via `.For<>()`. Routes follow the `api/{entityname}s` convention.

Use `EntityAutoEndpointsOptions` only for fully custom routes (e.g. versioning, renamed paths):
```csharp
app.MapEntityEndpoints(configure: options =>
{
    options.For<Product>("v2/shop/items"); // fully custom path
});
```

**Option B: Controller (when DTO mapping or custom behaviour is required)**

The generic type arguments on the controller must **exactly match** the type arguments used in `.For<>()`. 
The controller can add `TDto` and `TInputDto` on top.

| `.For<>()` registration | Required controller base |
|---|---|
| `.For<TEntity>()` | `EntityControllerBase<TEntity, TDto, TInputDto>` |
| `.For<TEntity, TKey>()` | `EntityControllerBase<TEntity, TKey, SearchObject<TKey>, TDto, TInputDto>` |
| `.For<TEntity, TKey, TSearchObject>()` | `EntityControllerBase<TEntity, TKey, TSearchObject, TDto, TInputDto>` |
| `.For<TEntity, TSearchObject, TSortBy, TIncludes>()` | `EntityControllerBase<TEntity, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>` |
| `.For<TEntity, TKey, TSearchObject, TSortBy, TIncludes>()` | `EntityControllerBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>` |

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Controllers

### Step 13: Update DbContext

Add `DbSet<YourEntity>` and configure any relationships in `OnModelCreating`.

> **→ See:** [`entities.examples.md`](./entities.examples.md) — DbContext

### Step 14: Setup and add Entity services to DI

> **→ See:** [`entities.setup.md`](./entities.setup.md) — Setup

- After changing `.For<>()` generic arguments, run the app once in addition to `dotnet build`.
- `dotnet build` verifies compilation, but startup DI validation is what catches mismatches between `.For<>()`, `EntityControllerBase<>`, and direct `IEntityService<...>` injections.

### Optional: Seed initial data

- Seed after `Database.EnsureCreated()` or after applying migrations, preferably in a startup scope.
- If you seed through `IEntityService`, the normal prepper and primer pipeline runs and you must still call `SaveChanges()` explicitly.
- If you inject `IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>` into a startup seeder, background job, or hosted service, its generic arguments must exactly match the `.For<>()` registration, including `TIncludes`.
- Use `DbContext` directly only when you intentionally want raw EF Core behavior instead of the entity-service pipeline.

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Additional Patterns > Database initialization and seeding

---

## Custom Entity Services

### Using EntityWrappingServiceBase

> Extends default logic of the Repositories. 
> The wrapper delegates all calls to an inner `IEntityService` and you override only what you need.
> Register via `e.UseEntityService<MyCustomEntityService>()` to replace the default repository.
⚠️ Prevent circular dependencies when injection parent EntityService!

Examples:
- Caching: Wrap the default `EntityRepository` with `IMemoryCache`.
  - Override `Details(id)` — check the cache first, then call `base.Details(id)` and store the result
  - Override `Save(item)` (and `Remove(item)` if needed) — call base, then invalidate the cache entry
- Security: e.g. modify the SearchObject using business rules to automatically filter results based on user permissions, without needing to add extra filters on every endpoint.
- Validation

**Registration:**
- `e.AddTransient<IProductService, ProductService>()` — enables typed injection by interface
- `e.UseEntityService<ProductService>()` — replaces the default `EntityRepository` as `IEntityService` for the entity

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Order + OrderLine entities (OrderManager)

### EntityWrappingServiceBase Available Overrides

```csharp
// Read
Task<TEntity?> Details(TKey id, CancellationToken token = default)
Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null, CancellationToken token = default)
Task<IList<TEntity>> List(IList<TSearchObject?> so, IList<TSortBy> sortBy, TIncludes? includes, PagingInfo? pagingInfo, CancellationToken token = default)
Task<long> Count(TSearchObject? so, CancellationToken token = default)
Task<long> Count(IList<TSearchObject?> so, CancellationToken token = default)

// Write
Task Save(TEntity item, CancellationToken token = default)
Task Add(TEntity item, CancellationToken token = default)
Task<TEntity?> Modify(TEntity item, CancellationToken token = default)
Task Remove(TEntity item, CancellationToken token = default)
Task<int> SaveChanges(CancellationToken token = default)
```

---

## Global Services

- Global services apply to **all entities implementing a given interface**.
- They are registered on the `EntityServiceCollectionOptions` (inside `UseEntities()`)
- Global services execute before entity-specific services — order matters

### Global Filter Query Builders

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Additional Patterns > Global filter query builder

### Global Preppers (Inline)

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Setup

### Global Primers

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Additional Patterns > Primers

### UseDefaults() — What It Registers

`options.UseDefaults()` is a convenience method that registers default primers, global query filters, and normalizer services.

> **→ See:** [Quick Reference: Built-in Services](#quick-reference-built-in-services) for the full list of registered classes.

---

## Normalizing

Normalization facilitates text search by removing diacritics, special characters, and standardizing whitespace.

### Attribute-Based (Recommended)

**`[Normalized]` attribute options:**

| Property | Purpose |
|----------|---------|
| `SourceProperty` | Single source property name |
| `SourceProperties` | Array of source property names (concatenated with space) |
| `Recursive` | Process nested objects (class-level, default: `true`) |
| `Normalizer` | Custom `INormalizer` or `IObjectNormalizer` type |

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Category entity

### Custom Normalizer

- Register per entity: `e.AddNormalizer<ProductNormalizer>()`
- Register globally: `options.AddNormalizer<IHasPhone, PhoneNormalizer>()`
- When `IsExclusive = true`, no other normalizer runs for that entity

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Additional Patterns > Global normalizer

### Filtering with Normalized Content and IQKeywordHelper

Use `IQKeywordHelper.Parse(q)` to parse `Q` into keywords with wildcard support (e.g. `"blue*"` → `"blue%"`). Use `keyword.QW` with `EF.Functions.Like`.

**Or use the built-in global filter** (applies to all `IHasNormalizedContent` entities):

```csharp
options.AddGlobalFilterQueryBuilder<FilterHasNormalizedContentQueryBuilder>();
```

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Additional Patterns > IQKeywordHelper — Q full-text search

### Enable Normalizer Interceptors

Normalizers run automatically when saving. Use the `(sp, options) =>` factory overload in `AddDbContext` and call `.AddNormalizerInterceptors(sp)`.

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Setup

---

## Attachments

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Additional Patterns > Attachments

### 1. EntityAttachment Model

Create a class inheriting `EntityAttachment<TKey, TObjectKey>` and override `ObjectType`.

### 2. Update Owning Entity

Implement `IHasAttachments` and `IHasAttachments<TAttachment>`. The `IHasAttachments.Attachments` property requires an explicit interface implementation.

### 3. Create Attachment Controller

Inherit from `EntityAttachmentControllerBase<TAttachment, TKey, TObjectKey>`. Route pattern: `api/{entity}/{objectId}/attachments`.

### 4. Update DbContext

Add `DbSet<Attachment>` and `DbSet<TAttachment>`. Configure the relationship between the attachment and its owning entity in `OnModelCreating`.

### 5. Configure DI

Call `.WithAttachments(_ => new BinaryFileService(...))` (or Azure/SFTP variant) on the `IEntityServiceCollection`.

---

## Error Handling

### EntityInputException (returns HTTP 400)

Controllers automatically catch `EntityInputException` and return `BadRequest (400)`.

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Order + OrderLine entities (OrderManager)

---

## Built-in Query Extensions

These LINQ extension methods are available for use inside query builders:

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Additional Patterns > Query extensions reference

---

## Common Patterns

### Master-Detail (Order + OrderItems)

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Order + OrderLine entities

### Many-to-Many Relations

**Treat Many-to-Many as two One-to-Many relations** using a middle/join table with an explicit join entity. Always create an explicit join entity — even if the join table carries no extra properties, having a dedicated entity makes the collection easier to manage via `e.Related()`.

- Use an explicit join entity and manage the collection via `e.Related()`
- Always configure the relationship in `DbContext.OnModelCreating`
- Use a prepper (or `.Related()`) to synchronize join table changes when updating
- Child entities registered via e.Related() do NOT need a standalone IEntityService<T> registration

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Product entity

### Soft Delete

Implement `IArchivable` on the entity. `UseDefaults()` automatically registers `ArchivablePrimer` and `FilterArchivablesQueryBuilder`.

`SearchObject.IsArchived`: `null` = active only (default), `false` = active only, `true` = archived only.

### Audit Trail with Custom Primer

Use a global `EntityPrimerBase<TInterface>` to stamp `CreatedBy`/`ModifiedBy` on every entity that implements a shared auditing interface. The primer runs inside EF Core's `SaveChanges` interceptor and resolves the current user via `IHttpContextAccessor`.

- Define an audit interface (e.g. `IAuditable`) with `CreatedBy` and `ModifiedBy` properties
- Implement `EntityPrimerBase<IAuditable>` — check `EntityState.Added` vs `Modified`
- Register globally via `options.AddPrimer<UserTrackingPrimer>()`
- Requires `AddPrimerInterceptors(sp)` in `AddDbContext`

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Additional Patterns > Primers

### Hierarchical Data (Self-referencing)

Add Parent, and Children navigation properties. Filter on `ParentId` or `ChildId` in the query builder; use `x.ParentId == null` to return only root items.

> **→ See:** [`entities.examples.md`](./entities.examples.md) — Category entity

### Auto truncate

Use `AddAutoTruncateInterceptors()` when registering DbContext to prevent string truncation exceptions

---

## Response Types

All base controller endpoints return standardized wrappers:

```csharp
// GET /api/entities/{id}
DetailsResult<TDto>  { TDto Item; long? Duration; }

// GET /api/entities
ListResult<TDto>     { IList<TDto> Items; long? Duration; }

// GET /api/entities/search  (Items + Count for pagination)
SearchResult<TDto>   { IList<TDto> Items; long Count; long? Duration; }

// POST / PUT / POST save
SaveResult<TDto>     { TDto Item; bool IsNew; int Affected; long? Duration; }

// DELETE /api/entities/{id}
DeleteResult<TDto>   { TDto Item; long? Duration; }
```

---

## Best Practices

- **Always call `SaveChanges()`** after write operations when using `IEntityService` directly (e.g. when seeding data) - this is standard EF Core behavior
- `Add()`, `Modify()`, `Remove()`, and `Save()` only track changes - they do NOT persist to the database
- Base controllers call `SaveChanges()` automatically after write endpoints (POST, PUT, DELETE)
- Custom code (services, background jobs, console apps) must explicitly call `await service.SaveChanges()` or `await dbContext.SaveChangesAsync()`
- Preppers run before `SaveChanges()` - they prepare entities for persistence but don't commit them
- Primers run during `SaveChanges()` via EF Core interceptors

---

## Quick Reference: Built-in Entity Interfaces

| Interface | Properties | Related Services |
|-----------|-----------|-----------------|
| `IEntity<TKey>` | `Id (TKey)` | `FilterIdsQueryBuilder` |
| `IEntityWithSerial` | `Id (int)` | *(same as `IEntity<int>`)* |
| `IHasCode` | `Code (string)` | Normalizers |
| `IHasTitle` | `Title (string)` | Normalizers, `FilterTitle` |
| `IHasDescription` | `Description (string)` | Normalizers |
| `IHasNormalizedContent` | `NormalizedContent (string)` | `FilterHasNormalizedContentQueryBuilder` |
| `IHasCreated` | `Created (DateTime)` | `HasCreatedDbPrimer`, `FilterHasCreatedQueryBuilder` |
| `IHasLastModified` | `LastModified (DateTime?)` | `HasLastModifiedDbPrimer`, `FilterHasLastModifiedQueryBuilder` |
| `IHasTimestamps` | `Created, LastModified` | Both timestamp services |
| `IArchivable` | `IsArchived (bool)` | `ArchivablePrimer`, `FilterArchivablesQueryBuilder` |
| `ISortable` | `SortOrder (int)` | `RelatedCollectionPrepper`, `EntityExtensions.SetSortOrder` |
| `IHasObjectId<TKey>` | `ObjectId (TKey)` | Attachments |
| `IHasAttachments` | `HasAttachment, Attachments` | Attachments module |

## Quick Reference: Built-in Services

These services are automatically registered when calling `options.UseDefaults()`, 
but can also be registered manually if you want to customize the configuration.

### Global Filter Query Builders

| Class | Applies to | Filters on |
|-------|-----------|-----------|
| `FilterIdsQueryBuilder` | All entities | `Id`, `Ids`, `Exclude` |
| `FilterArchivablesQueryBuilder` | `IArchivable` | `IsArchived` |
| `FilterHasCreatedQueryBuilder` | `IHasCreated` | `MinCreated`, `MaxCreated` |
| `FilterHasLastModifiedQueryBuilder` | `IHasLastModified` | `MinLastModified`, `MaxLastModified` |
| `FilterHasNormalizedContentQueryBuilder` | `IHasNormalizedContent` | `Q` keyword search |

### Primers

| Class | Applies to | Behaviour |
|-------|-----------|-----------|
| `HasCreatedDbPrimer` | `IHasCreated` | Sets `Created` on insert |
| `HasLastModifiedDbPrimer` | `IHasLastModified` | Sets `LastModified` on update |
| `ArchivablePrimer` | `IArchivable` | Soft-delete: sets `IsArchived = true` |
| `AutoTruncatePrimer` | All entities | Truncates strings to `[MaxLength]` |

### Normalizer Services

| Interface | Implementation | Role |
|-----------|---------------|------|
| `INormalizer` | `DefaultNormalizer` | Normalizes a string value |
| `IObjectNormalizer` | `ObjectNormalizer` | Processes `[Normalized]` attributes |
| `IEntityNormalizer` | `DefaultEntityNormalizer<IEntity>` | Orchestrates entity normalization |
| `IQKeywordHelper` | `QKeywordHelper` | Parses Q search strings with wildcard support |

---

## Troubleshooting

Always use the latest .net version (net10 atm) unless rquested otherwise.

| Problem | Likely Cause | Fix |
|---------|-------------|-----|
| Navigation properties not loaded | Missing `Includes` config or wrong flag | Check `e.Includes(...)` and that the client sends the correct `includes` flag |
| Filter not applied | Query builder not registered or wrong `SearchObject` property name | Verify `e.AddFilter<>()` or `e.Filter(...)` and check property names |
| Mapping errors | Mapster/AutoMapper not configured or property name mismatch | Ensure `options.UseMapsterMapping()` is called; check DTO property names |
| Normalizer not running | `AddNormalizerInterceptors(sp)` missing or wrong overload | Use `(sp, options) =>` factory overload in `AddDbContext` |
| Primers not running | `AddPrimerInterceptors(sp)` missing | Same as above |
| Save not persisting | `SaveChanges()` not called | ⚠️ **EF Core Pattern**: Write operations only track changes. Must call `SaveChanges()` to persist. Base controllers do this automatically; custom code (services, jobs, direct `IEntityService` usage) must call `await service.SaveChanges()` explicitly. |
| `List(null)` compiler error | Ambiguous overload between typed and untyped variants | Omit the argument (`service.List()`) or cast: `service.List((TSearchObject?)null)`. `Count` is not affected — use `await service.Count()` with no arguments. |
| `SetSortOrder()` does not compile on `ICollection<T>` | Extension targets `IEnumerable<ISortable>`, not `ICollection<T>` | Use `items.SetSortOrder()` when `T : ISortable` (generic overload), or cast: `(items as IEnumerable<ISortable>)?.SetSortOrder()` |
| Soft delete not working | `IArchivable` not implemented or `ArchivablePrimer` not registered | Check entity implements `IArchivable`; use `UseDefaults()` |
| `AddPrimerInterceptors` has no overload taking 0 args | Missing `IServiceProvider` | Use `AddDbContext<T>((sp, options) => ...)` and pass `sp` |
| `EntityWrappingServiceBase` — infinite loop | Inner service is the wrapper itself | Ensure `UseEntityService<T>()` registers the wrapper; `AddTransient` registers the interface |
| CS0246: type or namespace name could not be found | Namespace guessed or copied from wrong source | Look up the exact namespace in [`entities.namespaces.md`](./entities.namespaces.md) |
| Wrong method name, parameters, or return type | Signature guessed or assumed | Look up the exact signature in [`entities.signatures.md`](./entities.signatures.md) |
| Unsure how to implement a pattern or need a working example | No reference at hand | Copy from the matching section in [`entities.examples.md`](./entities.examples.md) |
| `dotnet restore` fails with "Detected package downgrade" | Project targets a framework that is lower than what a dependency requires | Use latest `<TargetFramework>` in the `.csproj` |

---

## See Also

- [Entities Examples](./entities.examples.md) - Code examples and patterns
- [Entities Namespaces](./entities.namespaces.md) - Namespace reference
- [Entities Signatures](./entities.signatures.md) - Exact method signatures for all interfaces and classes
