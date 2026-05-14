# Regira Entities Framework - API Signatures Reference

Exact signatures for interfaces, classes, and extension methods in the Regira Entities framework. **Do not guess — look up here first.**

---

## Table of Contents

1. [Entity Interfaces](#entity-interfaces)
2. [Service Interfaces](#service-interfaces)
3. [Controller Base Classes](#controller-base-classes)
4. [FastEndpoints (Minimal API)](#fastendpoints-minimal-api)
5. [Search and Filter Objects](#search-and-filter-objects)
6. [Extension Methods](#extension-methods)
7. [Service Builders](#service-builders)
8. [Mapping and Processing](#mapping-and-processing)
9. [Response Types](#response-types)
10. [Attachments](#attachments)
11. [Exceptions](#exceptions)
12. [Supporting Types](#supporting-types)

---

## Entity Interfaces

### Core Entity Interfaces

```csharp
using Regira.Entities.Models.Abstractions;

public interface IEntity;
public interface IEntity<TKey> : IEntity
{
    TKey Id { get; set; }
}

// Shortcut for int primary key
public interface IEntityWithSerial : IEntity<int>;
```

### Property Interfaces

```csharp
using Regira.Entities.Models.Abstractions;

public interface IHasCode
{
    string? Code { get; set; }
}
// Title is read-only on the interface; entity classes may expose a setter
public interface IHasTitle
{
    string? Title { get; }
}
public interface IHasNormalizedTitle : IHasTitle
{
    string? NormalizedTitle { get; set; }
}
public interface IHasDescription
{
    string? Description { get; set; }
}
public interface IHasNormalizedContent
{
    string? NormalizedContent { get; set; }
}
public interface IHasLastNormalized : IHasNormalizedContent, IHasLastModified
{
    DateTime? LastNormalized { get; set; }
}
public interface ISortable
{
    int SortOrder { get; set; }
}
```

### Timestamp Interfaces

```csharp
using Regira.Entities.Models.Abstractions;

public interface IHasCreated
{
    DateTime Created { get; set; }
}
public interface IHasLastModified
{
    DateTime? LastModified { get; set; }
}

// Combined — most common choice
public interface IHasTimestamps : IHasCreated, IHasLastModified;
```

### Lifecycle Interface

```csharp
using Regira.Entities.Models.Abstractions;

public interface IArchivable
{
    bool IsArchived { get; set; }
}
```

### Attachment Interfaces

```csharp
using Regira.Entities.Attachments.Abstractions;

public interface IHasAttachments
{
    ICollection<IEntityAttachment>? Attachments { get; set; }
    bool? HasAttachment { get; set; }
}

// Typed (int keys) — most common
public interface IHasAttachments<TEntityAttachment>
    : IHasAttachments<TEntityAttachment, int, int, int, Attachment>
    where TEntityAttachment : IEntityAttachment<int, int, int, Attachment>;

public interface IHasAttachments<TEntityAttachment, TKey, TObjectKey, TAttachmentKey, TAttachment>
    where TEntityAttachment : IEntityAttachment<TKey, TObjectKey, TAttachmentKey, TAttachment>
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
{
    ICollection<TEntityAttachment>? Attachments { get; set; }
    bool? HasAttachment { get; set; }
}

public interface IHasObjectId<TKey>
{
    TKey ObjectId { get; set; }
}
```

---

## Service Interfaces

### Read

```csharp
using Regira.Entities.Services.Abstractions;

public interface IEntityReadService<TEntity, in TKey>
{
    Task<TEntity?> Details(TKey id, CancellationToken token = default);
    Task<IList<TEntity>> List(object? so = null, PagingInfo? pagingInfo = null, CancellationToken token = default);
    Task<long> Count(object? so, CancellationToken token = default);
}

public interface IEntityReadService<TEntity, in TKey, in TSearchObject>
    : IEntityReadService<TEntity, TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null, CancellationToken token = default);
    Task<long> Count(TSearchObject? so = null, CancellationToken token = default);
}

public interface IEntityReadService<TEntity, in TKey, TSearchObject, TSortBy, TIncludes>
    : IEntityReadService<TEntity, TKey, TSearchObject>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    Task<IList<TEntity>> List(
        IList<TSearchObject?> so,
        IList<TSortBy> sortBy,
        TIncludes? includes = null,
        PagingInfo? pagingInfo = null,
        CancellationToken token = default);
    Task<long> Count(IList<TSearchObject?> so, CancellationToken token = default);
}
```

### Write

```csharp
using Regira.Entities.Services.Abstractions;

public interface IEntityWriteService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    Task Add(TEntity item, CancellationToken token = default);
    Task<TEntity?> Modify(TEntity item, CancellationToken token = default);
    Task Save(TEntity item, CancellationToken token = default);  // Upsert: Add or Modify
    Task Remove(TEntity item, CancellationToken token = default);
    Task<int> SaveChanges(CancellationToken token = default);
}
```

### Combined (IEntityService)

```csharp
using Regira.Entities.Services.Abstractions;

// Full-featured (TKey explicit)
public interface IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    : IEntityReadService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>,
      IEntityService<TEntity, TKey, TSearchObject>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;

// Shortcut — int key assumed (most common for injection)
public interface IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>
    : IEntityService<TEntity, int, TSearchObject, TSortBy, TIncludes>,
      IEntityService<TEntity>
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;
```

> **Inject** as `IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>` (int key shortcut) when registered with `.For<TEntity, TSearchObject, TSortBy, TIncludes>()`.

### IEntityRepository / IEntityManager

Custom services with `HasRepository<>()` or `HasManager<>()`.

```csharp
using Regira.Entities.Services.Abstractions;

// Primary shortcut forms (int key, full-featured)
public interface IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>
    : IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityRepository<TEntity>;

public interface IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>
    : IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityManager<TEntity>;

// Additional TKey and partial variants follow the same pattern as IEntityService.
```

### EntityWrappingServiceBase

Inject the inner service via constructor; override only the methods you need.

```csharp
using Regira.Entities.Services.Abstractions;

// Without sort/includes — exposes Service field
public abstract class EntityWrappingServiceBase<TEntity, TKey, TSearchObject>(
    IEntityService<TEntity, TKey, TSearchObject> service) : IEntityService<TEntity, TKey, TSearchObject>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    protected readonly IEntityService<TEntity, TKey, TSearchObject> Service = service;

    public virtual Task<TEntity?> Details(TKey id, CancellationToken token = default);
    public virtual Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null, CancellationToken token = default);
    public virtual Task<long> Count(TSearchObject? so, CancellationToken token = default);
    public virtual Task Add(TEntity item, CancellationToken token = default);
    public virtual Task<TEntity?> Modify(TEntity item, CancellationToken token = default);
    public virtual Task Save(TEntity item, CancellationToken token = default);
    public virtual Task Remove(TEntity item, CancellationToken token = default);
    public virtual Task<int> SaveChanges(CancellationToken token = default);
}

// Full-featured (int key) — most common; empty body, delegates to explicit-TKey variant
public abstract class EntityWrappingServiceBase<TEntity, TSearchObject, TSortBy, TIncludes>(
    IEntityService<TEntity, int, TSearchObject, TSortBy, TIncludes> service)
    : EntityWrappingServiceBase<TEntity, int, TSearchObject, TSortBy, TIncludes>(service)
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;

// Full-featured (explicit TKey)
public abstract class EntityWrappingServiceBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes>(
    IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes> service)
    : IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    // All IEntityService members are virtual — override as needed:
    public virtual Task<TEntity?> Details(TKey id, CancellationToken token = default);
    public virtual Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null, CancellationToken token = default);
    public virtual Task<IList<TEntity>> List(IList<TSearchObject?> so, IList<TSortBy> sortBy, TIncludes? includes = null, PagingInfo? pagingInfo = null, CancellationToken token = default);
    public virtual Task<long> Count(TSearchObject? so, CancellationToken token = default);
    public virtual Task<long> Count(IList<TSearchObject?> so, CancellationToken token = default);
    public virtual Task Add(TEntity item, CancellationToken token = default);
    public virtual Task<TEntity?> Modify(TEntity item, CancellationToken token = default);
    public virtual Task Save(TEntity item, CancellationToken token = default);
    public virtual Task Remove(TEntity item, CancellationToken token = default);
    public virtual Task<int> SaveChanges(CancellationToken token = default);
    public virtual TSearchObject? Convert(object? so);
}
```

> Register the wrapper with `e.UseEntityService<MyService>()` (**⚠️ Beware for circular dependency!**).
> You can register implementations for custom interface derived from `IEntityService` with `e.AddTransient<IMyService, MyService>()`.

---

## Controller Base Classes

The generic type arguments on the controller must **exactly match** those used in `.For<>()`. The controller adds `TDto` and `TInputDto` on top.

| `.For<>()` registration | Required controller base |
|---|---|
| `.For<TEntity>()` | `EntityControllerBase<TEntity, TDto, TInputDto>` |
| `.For<TEntity, TKey>()` | `EntityControllerBase<TEntity, TKey, SearchObject<TKey>, TDto, TInputDto>` |
| `.For<TEntity, TKey, TSearchObject>()` | `EntityControllerBase<TEntity, TKey, TSearchObject, TDto, TInputDto>` |
| `.For<TEntity, TSearchObject, TSortBy, TIncludes>()` | `EntityControllerBase<TEntity, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>` |
| `.For<TEntity, TKey, TSearchObject, TSortBy, TIncludes>()` | `EntityControllerBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>` |

```csharp
using Regira.Entities.Web.Controllers.Abstractions;

// Minimal — no sorting or includes
[ApiController]
public abstract class EntityControllerBase<TEntity, TKey, TSearchObject, TDto, TInputDto>
    : ControllerBase
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>
    where TDto : class
    where TInputDto : class;

// Full-featured — with sorting and includes (int key shortcut)
[ApiController]
public abstract class EntityControllerBase<TEntity, TSo, TSortBy, TIncludes, TDto, TInputDto>
    : EntityControllerBase<TEntity, int, TSo, TSortBy, TIncludes, TDto, TInputDto>
    where TEntity : class, IEntity<int>
    where TSo : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
    where TDto : class
    where TInputDto : class;

// Full-featured — with explicit TKey
[ApiController]
public abstract class EntityControllerBase<TEntity, TKey, TSo, TSortBy, TIncludes, TDto, TInputDto>
    : ControllerBase
    where TEntity : class, IEntity<TKey>
    where TSo : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
    where TDto : class
    where TInputDto : class;
```

**Endpoints exposed by all controller variants:**

| Method | Route | Action |
|--------|-------|--------|
| `GET` | `/{id}` | Details |
| `GET` | `/` | List |
| `POST` | `/list` | List (body) |
| `GET` | `/search` | Search (with count) |
| `POST` | `/search` | Search (body) |
| `POST` | `/save` | Save (upsert) |
| `POST` | `/` | Create |
| `PUT` | `/{id}` | Modify |
| `DELETE` | `/{id}` | Delete |

---

## FastEndpoints (Minimal API)

NuGet: `Regira.Entities.Web.FastEndpoints`

### Auto-Registration

```csharp
using Regira.Entities.Web.FastEndpoints.Extensions;
using Regira.Entities.Web.FastEndpoints.Models;

// Scans DI for all IEntityService<,> registrations and maps CRUD routes for each entity.
// Requires UseEntities<TContext>() to have been called during service registration.
WebApplication MapEntityEndpoints(
    this WebApplication app,
    string routePrefix = "api",
    Action<EntityAutoEndpointsOptions>? configure = null)
```

**Routes registered per entity** (base: `{routePrefix}/{entityName}s`):

| Method | Route | Action |
|--------|-------|--------|
| `GET` | `/{id}` | Details (404 if missing) |
| `GET` | `/` | List (`?page`, `?pageSize`) |
| `POST` | `/` | Create |
| `POST` | `/save` | Save (upsert; 404 if updating nonexistent) |
| `PUT` | `/{id}` | Modify (404 if missing) |
| `DELETE` | `/{id}` | Delete (404 if missing) |

> Note: auto-registration uses raw entity classes (no DTOs/mapping). Use the endpoint base classes below for DTO-aware endpoints.

### Configuration

```csharp
using Regira.Entities.Web.FastEndpoints.Models;

public class EntityAutoEndpointsOptions
{
    // Default route prefix. Default: "api"
    public string RoutePrefix { get; set; }

    // Override the route for a specific entity (for fully custom paths; irregular plurals are handled automatically)
    public EntityAutoEndpointsOptions For<TEntity>(string route)
        where TEntity : class, IEntity;

    // Returns the configured route, falling back to {RoutePrefix}/{entityName}s
    public string GetRouteFor(Type entityType);
}
```

### Endpoint Base Classes (manual, DTO-aware)

```csharp
using Regira.Entities.Web.FastEndpoints.Endpoints.Abstractions;

// Simple endpoints (int-key convenience variants available — omit TKey)
EntityDetailsEndpointBase<TEntity, TKey, TDto>
EntityListEndpointBase<TEntity, TKey, TSearchObject, TDto>
EntityCreateEndpointBase<TEntity, TKey, TDto, TInputDto>
EntitySaveEndpointBase<TEntity, TKey, TDto, TInputDto>
EntityModifyEndpointBase<TEntity, TKey, TDto, TInputDto>
EntityDeleteEndpointBase<TEntity, TKey, TDto>

// Complex endpoints (with TSortBy + TIncludes; int-key convenience variants available)
ComplexEntityListGetEndpointBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto>
ComplexEntityListPostEndpointBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto>
ComplexEntitySearchGetEndpointBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto>
ComplexEntitySearchPostEndpointBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto>
```

Override `Configure()` to add authentication, OpenAPI tags, or other FastEndpoints settings.

---

## Search and Filter Objects

```csharp
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Models;

public interface ISearchObject
{
    string? Q { get; set; }
    DateTime? MinCreated { get; set; }
    DateTime? MaxCreated { get; set; }
    DateTime? MinLastModified { get; set; }
    DateTime? MaxLastModified { get; set; }
    bool? IsArchived { get; set; }
}

public interface ISearchObject<TKey> : ISearchObject
{
    TKey? Id { get; set; }
    ICollection<TKey>? Ids { get; set; }
    ICollection<TKey>? Exclude { get; set; }
}

// Default implementation (int key) — extend this for custom filters
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

---

## Extension Methods

### EntityExtensions

```csharp
using Regira.Entities.Extensions;

public static class EntityExtensions
{
    public static bool IsNew<TKey>(this IEntity<TKey> item);
    public static void AdjustIdForEfCore(this IEnumerable<IEntity<int>> items);
    public static void SetSortOrder<T>(this IEnumerable<T> items) where T : ISortable;
}
```

### QueryExtensions

> Every method

```csharp
using Regira.Entities.EFcore.Extensions;

public static class QueryExtensions
{
    // Requires IEntity<TKey>
    public static IQueryable<TEntity> FilterId<TEntity, TKey>(this IQueryable<TEntity> query, TKey? id);
    public static IQueryable<TEntity> FilterIds<TEntity, TKey>(this IQueryable<TEntity> query, ICollection<TKey>? ids);
    public static IQueryable<TEntity> FilterExclude<TEntity, TKey>(this IQueryable<TEntity> query, ICollection<TKey>? ids);

    // Requires IHasCode
    public static IQueryable<TEntity> FilterCode<TEntity>(this IQueryable<TEntity> query, string? code);

    // Requires IHasTitle
    public static IQueryable<TEntity> FilterTitle<TEntity>(this IQueryable<TEntity> query, ParsedKeywordCollection? keywords);

    // Requires IHasNormalizedTitle
    public static IQueryable<TEntity> FilterNormalizedTitle<TEntity>(this IQueryable<TEntity> query, ParsedKeywordCollection? keywords);

    // Requires IHasNormalizedContent
    public static IQueryable<TEntity> FilterQ<TEntity>(this IQueryable<TEntity> query, ParsedKeywordCollection? keywords);

    // Requires IHasCreated
    public static IQueryable<TEntity> FilterCreated<TEntity>(this IQueryable<TEntity> query, DateTime? minDate, DateTime? maxDate);

    // Requires IHasLastModified
    public static IQueryable<TEntity> FilterLastModified<TEntity>(this IQueryable<TEntity> query, DateTime? minDate, DateTime? maxDate);

    // Requires IHasTimestamps
    public static IQueryable<TEntity> FilterTimestamps<TEntity>(this IQueryable<TEntity> query,
        DateTime? minCreated, DateTime? maxCreated, DateTime? minModified, DateTime? maxModified);

    // Requires IArchivable
    public static IQueryable<TEntity> FilterArchivable<TEntity>(this IQueryable<TEntity> query, bool? isArchived);

    // Requires IHasAttachments
    public static IQueryable<TEntity> FilterHasAttachment<TEntity>(this IQueryable<TEntity> query, bool? hasAttachment);

    public static IQueryable<TEntity> SortQuery<TEntity, TKey>(this IQueryable<TEntity> query)
        where TEntity : IEntity<TKey>;
}
```

---

## Service Builders

### Top-Level DI Entry Point

```csharp
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;

public static EntityServiceCollection<TContext> UseEntities<TContext>(
    this IServiceCollection services,
    Action<EntityServiceCollectionOptions>? configure = null)
    where TContext : DbContext;
```

---

### EntityServiceCollectionOptions Extension Methods

#### Setup

```csharp
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;

// Registers default primers
public static EntityServiceCollectionOptions UseDefaults(
    this EntityServiceCollectionOptions options,
    Action<EntityDefaultNormalizingOptions>? configure = null);

public static EntityServiceCollectionOptions UseNormalizerDefaults(
    this EntityServiceCollectionOptions options,
    Action<EntityDefaultNormalizingOptions>? configure = null);
```

#### Mapping

```csharp
// package: Regira.Entities.Mapping.Mapster
using Regira.Entities.Mapping.Mapster;

public static EntityServiceCollectionOptions UseMapsterMapping(
    this EntityServiceCollectionOptions options,
    Action<TypeAdapterConfig>? configure = null);
```

```csharp
// package: Regira.Entities.Mapping.AutoMapper
using Regira.Entities.Mapping.AutoMapper;

public static EntityServiceCollectionOptions UseAutoMapper(
    this EntityServiceCollectionOptions options,
    Action<IServiceProvider, IMapperConfigurationExpression>? configure = null);
```

```csharp
using Regira.Entities.DependencyInjection.Mapping;

public static EntityServiceCollectionOptions AddAfterMapper<TAfterMapper>(
    this EntityServiceCollectionOptions options)
    where TAfterMapper : class, IEntityAfterMapper;

public static EntityServiceCollectionOptions AfterMap<TSource, TTarget>(
    this EntityServiceCollectionOptions options,
    Action<TSource, TTarget> afterMapAction);

public static EntityServiceCollectionOptions AfterMap<TSource, TTarget>(
    this EntityServiceCollectionOptions options,
    Func<IServiceProvider, Action<TSource, TTarget>> afterMapAction);
```

#### Preppers (global)

```csharp
using Regira.Entities.DependencyInjection.Preppers;

public static EntityServiceCollectionOptions AddPrepper<TImplementation>(
    this EntityServiceCollectionOptions options)
    where TImplementation : class, IEntityPrepper;

public static EntityServiceCollectionOptions AddPrepper<TEntity>(
    this EntityServiceCollectionOptions options,
    Action<TEntity> prepareFunc)
    where TEntity : class;

public static EntityServiceCollectionOptions AddPrepper<TContext, TEntity, TKey>(
    this EntityServiceCollectionOptions options,
    Func<TEntity, TContext, Task> prepareFunc)
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>;
```

#### Primers (global)

```csharp
using Regira.Entities.DependencyInjection.Primers;

public static EntityServiceCollectionOptions AddPrimer<TPrimer>(
    this EntityServiceCollectionOptions options)
    where TPrimer : class, IEntityPrimer;

// Registers ArchivablePrimer + HasCreatedDbPrimer + HasLastModifiedDbPrimer
public static EntityServiceCollectionOptions AddDefaultPrimers(
    this EntityServiceCollectionOptions options);
```

#### Global Filter Query Builders

```csharp
using Regira.Entities.DependencyInjection.QueryBuilders;

public static EntityServiceCollectionOptions AddGlobalFilterQueryBuilder<TImplementation>(
    this EntityServiceCollectionOptions options)
    where TImplementation : class, IGlobalFilteredQueryBuilder;

// Registers FilterIdsQueryBuilder + FilterArchivablesQueryBuilder
//   + FilterHasCreatedQueryBuilder + FilterHasLastModifiedQueryBuilder
public static EntityServiceCollectionOptions AddDefaultGlobalQueryFilters(
    this EntityServiceCollectionOptions options);
```

#### Normalizers (global)

```csharp
using Regira.Entities.DependencyInjection.Normalizers;

public static EntityServiceCollectionOptions AddNormalizer<TNormalizer>(
    this EntityServiceCollectionOptions options)
    where TNormalizer : class, IEntityNormalizer;

public static EntityServiceCollectionOptions AddNormalizer<TEntity, TNormalizer>(
    this EntityServiceCollectionOptions options)
    where TNormalizer : class, IEntityNormalizer<TEntity>;

// Registers DefaultNormalizer + ObjectNormalizer + DefaultEntityNormalizer + QKeywordHelper
public static EntityServiceCollectionOptions AddDefaultEntityNormalizer(
    this EntityServiceCollectionOptions options,
    Action<NormalizeOptions>? configure = null);
```

---

### EntityServiceCollection\<TContext\>

```csharp
using Regira.Entities.DependencyInjection.ServiceBuilders;

public class EntityServiceCollection<TContext>
    where TContext : DbContext
{
    // Simple entity (int key, default SearchObject)
    EntityServiceCollection<TContext> For<TEntity>(
        Action<EntityIntServiceBuilder<TContext, TEntity>>? configure = null)
        where TEntity : class, IEntity<int>;

    // Entity with custom TKey
    EntityServiceCollection<TContext> For<TEntity, TKey>(
        Action<EntityServiceBuilder<TContext, TEntity, TKey>>? configure = null)
        where TEntity : class, IEntity<TKey>;

    // Entity with custom TKey + SearchObject
    EntityServiceCollection<TContext> For<TEntity, TKey, TSearchObject>(
        Action<EntitySearchObjectServiceBuilder<TContext, TEntity, TKey, TSearchObject>>? configure = null)
        where TEntity : class, IEntity<TKey>
        where TSearchObject : class, ISearchObject<TKey>, new();

    // Full-featured (int key) — most common
    EntityServiceCollection<TContext> For<TEntity, TSearchObject, TSortBy, TIncludes>(
        Action<ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes>>? configure = null)
        where TEntity : class, IEntity<int>
        where TSearchObject : class, ISearchObject<int>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum;

    // Full-featured (with TKey)
    EntityServiceCollection<TContext> For<TEntity, TKey, TSearchObject, TSortBy, TIncludes>(
        Action<ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>>? configure = null)
        where TEntity : class, IEntity<TKey>
        where TSearchObject : class, ISearchObject<TKey>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum;

    // Attachments
    EntityServiceCollection<TContext> WithAttachments(
        Func<IServiceProvider, IFileService> factory,
        Action<EntitySearchObjectServiceBuilder<TContext, Attachment, int, AttachmentSearchObject<int>>>? configure = null);

    // Generic service helpers
    EntityServiceCollection<TContext> AddTransient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;

    EntityServiceCollection<TContext> AddTransient<TService>(
        Func<IServiceProvider, TService> factory)
        where TService : class;
}
```

---

### EntityServiceBuilder\<TContext, TEntity, TKey\>

Base builder.

```csharp
using Regira.Entities.DependencyInjection.ServiceBuilders;

public partial class EntityServiceBuilder<TContext, TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    bool HasEntityService();
    bool HasService<TService>();

    // Elevate to typed search object builder
    EntitySearchObjectServiceBuilder<TContext, TEntity, TKey, TSearchObject>
        WithSearchObject<TSearchObject>()
        where TSearchObject : class, ISearchObject<TKey>, new();

    // Mapping
    MappedEntityServiceBuilder<TContext, TEntity, TKey, TDto, TInputDto>
        UseMapping<TDto, TInputDto>(Action<IEntityMapConfigurator>? mapAction = null);

    // Register additional type pairs for nested DTOs / related collections.
    // <A, A> same-type: registers the DTO so nested collections are mapped in output
    //   (e.g. AddMapping<CategoryDto, CategoryDto>() — mapper knows how to project the list).
    // <InputDto, Entity> cross-type: required when InputDto items appear inside a Related()
    //   navigation — without this pair the mapper cannot resolve the input items to entities.
    // e.g. e.AddMapping<ProductCategoryDto, ProductCategoryDto>() — same-type registers nested DTO
    //      e.AddMapping<ProductCategoryInputDto, ProductCategory>() — input → entity for Related()
    EntityServiceBuilder<TContext, TEntity, TKey> AddMapping<TSource, TTarget>();

    // Service registration
    EntityServiceBuilder<TContext, TEntity, TKey> AddDefaultService();

    EntityServiceBuilder<TContext, TEntity, TKey> UseEntityService<TService>()
        where TService : class, IEntityService<TEntity, TKey>, IEntityService<TEntity, TKey, SearchObject<TKey>>;

    EntityServiceBuilder<TContext, TEntity, TKey> UseEntityService<TService>(
        Func<IServiceProvider, TService> factory)
        where TService : class, IEntityService<TEntity, TKey, SearchObject<TKey>>;

    EntityServiceBuilder<TContext, TEntity, TKey> UseReadService<TService>()
        where TService : class, IEntityReadService<TEntity, TKey, SearchObject<TKey>>;

    EntityServiceBuilder<TContext, TEntity, TKey> UseWriteService<TService>()
        where TService : class, IEntityWriteService<TEntity, TKey>;

    EntityServiceBuilder<TContext, TEntity, TKey> HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity, TKey, SearchObject<TKey>>;

    EntityServiceBuilder<TContext, TEntity, TKey> HasRepository<TImplementation>(
        Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity, TKey>, IEntityRepository<TEntity, TKey, SearchObject<TKey>>;

    EntityServiceBuilder<TContext, TEntity, TKey> HasManager<TService>()
        where TService : class, IEntityManager<TEntity, TKey>, IEntityManager<TEntity, TKey, SearchObject<TKey>>;

    EntityServiceBuilder<TContext, TEntity, TKey> HasManager<TImplementation>(
        Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityManager<TEntity, TKey>, IEntityManager<TEntity, TKey, SearchObject<TKey>>;

    // Query builders
    EntityServiceBuilder<TContext, TEntity, TKey> AddDefaultQueryBuilder();

    EntityServiceBuilder<TContext, TEntity, TKey> UseQueryBuilder<TImplementation>()
        where TImplementation : class, IQueryBuilder<TEntity, TKey, SearchObject<TKey>, EntitySortBy, EntityIncludes>;

    EntityServiceBuilder<TContext, TEntity, TKey> UseQueryBuilder<TImplementation>(
        Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IQueryBuilder<TEntity, TKey, SearchObject<TKey>, EntitySortBy, EntityIncludes>;

    EntityServiceBuilder<TContext, TEntity, TKey> AddFilter<TImplementation>()
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, SearchObject<TKey>>;

    EntityServiceBuilder<TContext, TEntity, TKey> AddFilter<TImplementation>(
        Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, SearchObject<TKey>>;

    EntityServiceBuilder<TContext, TEntity, TKey> Filter(
        Func<IQueryable<TEntity>, SearchObject<TKey>?, IQueryable<TEntity>> filterFunc);

    // Sorting / includes (typed to EntitySortBy / EntityIncludes at this level)
    EntityServiceBuilder<TContext, TEntity, TKey> SortBy(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> sortBy);

    EntityServiceBuilder<TContext, TEntity, TKey> Includes(
        Func<IQueryable<TEntity>, EntityIncludes?, IQueryable<TEntity>> addIncludes);

    EntityServiceBuilder<TContext, TEntity, TKey> AddPrimer<TPrimer>()
        where TPrimer : class, IEntityPrimer<TEntity>;

    // Normalizers
    EntityServiceBuilder<TContext, TEntity, TKey> AddNormalizer<TNormalizer>()
        where TNormalizer : class, IEntityNormalizer<TEntity>;

    // Processors
    EntityServiceBuilder<TContext, TEntity, TKey> Process(
        Func<IList<TEntity>, EntityIncludes?, Task> process);

    EntityServiceBuilder<TContext, TEntity, TKey> Process(
        Action<TEntity, EntityIncludes?> process);

    EntityServiceBuilder<TContext, TEntity, TKey> AddProcessor<TProcessor>()
        where TProcessor : class, IEntityProcessor<TEntity, EntityIncludes>;

    // Preppers
    // inline shortcuts:
    EntityServiceBuilder<TContext, TEntity, TKey> Prepare(Action<TEntity> prepareFunc);

    EntityServiceBuilder<TContext, TEntity, TKey> Prepare(
        Func<TEntity, TContext, Task> prepareFunc);

    // class-based:
    EntityServiceBuilder<TContext, TEntity, TKey> AddPrepper<TPrepper>()
        where TPrepper : class, IEntityPrepper<TEntity>;

    // Primers
    // inline shortcuts:
    EntityServiceBuilder<TContext, TEntity, TKey> Prime(Action<TEntity> primeFunc);

    EntityServiceBuilder<TContext, TEntity, TKey> Prime(
        Func<TEntity, EntityEntry, TContext, Task> primeFunc);

    // class-based (moved here from above)

    // Related child collections (managed by RelatedCollectionPrepper)
    EntityServiceBuilder<TContext, TEntity, TKey> Related<TRelated, TRelatedKey>(
        Expression<Func<TEntity, ICollection<TRelated>?>> navigationExpression,
        Action<TEntity>? prepareFunc = null)
        where TRelated : class, IEntity<TRelatedKey>;

    void Build();
}
```

---

### EntitySearchObjectServiceBuilder

Returned by `WithSearchObject<TSearchObject>()`. Inherits all `EntityServiceBuilder` methods.
**Only listing new / changed members:**

```csharp
public partial class EntitySearchObjectServiceBuilder<TContext, TEntity, TKey, TSearchObject>
    : EntityServiceBuilder<TContext, TEntity, TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    // NEW: elevate to full-featured builder
    ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>
        Complex<TSortBy, TIncludes>()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum;

    // CHANGED: constraint uses TSearchObject instead of SearchObject<TKey>
    EntitySearchObjectServiceBuilder<...> UseEntityService<TService>()
        where TService : class, IEntityService<TEntity, TKey, TSearchObject>;

    EntitySearchObjectServiceBuilder<...> AddFilter<TImplementation>()
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>;

    EntitySearchObjectServiceBuilder<...> AddFilter<TImplementation>(
        Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>;

    EntitySearchObjectServiceBuilder<...> Filter(
        Func<IQueryable<TEntity>, TSearchObject?, IQueryable<TEntity>> filterFunc);

    void Build();
}
```

---

### Int-Key Variants

```csharp
// For<TEntity>() → EntityIntServiceBuilder
public partial class EntityIntServiceBuilder<TContext, TEntity>
    : EntityServiceBuilder<TContext, TEntity, int>
    where TEntity : class, IEntity<int>
{
    // Advance to SearchObject variant
    EntityIntServiceBuilder<TContext, TEntity, TSearchObject> WithSearchObject<TSearchObject>()
        where TSearchObject : class, ISearchObject<int>, new();

    // Int-key shortcuts (no TRelatedKey / TContext parameter needed)
    EntityIntServiceBuilder<TContext, TEntity> Prepare(Func<TEntity, TContext, Task> prepareFunc);

    EntityIntServiceBuilder<TContext, TEntity> Related<TRelated>(
        Expression<Func<TEntity, ICollection<TRelated>?>> navigationExpression,
        Action<TEntity>? prepareFunc = null)
        where TRelated : class, IEntity<int>;

    void Build();
}

// For<TEntity, TSearchObject>() or WithSearchObject() → EntityIntServiceBuilder<TContext, TEntity, TSearchObject>
public class EntityIntServiceBuilder<TContext, TEntity, TSearchObject>
    : EntitySearchObjectServiceBuilder<TContext, TEntity, int, TSearchObject>
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
{
    // Advance to full-featured
    ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes>
        Complex<TSortBy, TIncludes>()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum;

    void Build();
}
```

---

### ComplexEntityServiceBuilder

Returned by `.For<TEntity, TKey, TSearchObject, TSortBy, TIncludes>()` or `.Complex<TSortBy, TIncludes>()`.
Inherits all `EntitySearchObjectServiceBuilder` methods.
**Only listing new / changed members:**

```csharp
public partial class ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    : EntitySearchObjectServiceBuilder<TContext, TEntity, TKey, TSearchObject>
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    // CHANGED: constraints use full TSortBy/TIncludes
    ComplexEntityServiceBuilder<...> UseEntityService<TService>()
        where TService : class, IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>;

    ComplexEntityServiceBuilder<...> UseReadService<TService>()
        where TService : class, IEntityReadService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>;

    ComplexEntityServiceBuilder<...> HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, IEntityRepository<TEntity, TKey>;

    ComplexEntityServiceBuilder<...> HasManager<TService>()
        where TService : class, IEntityManager<TEntity, TKey, TSearchObject, TSortBy, TIncludes>;

    ComplexEntityServiceBuilder<...> UseQueryBuilder<TImplementation>()
        where TImplementation : class, IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>;

    // NEW: typed sorting
    ComplexEntityServiceBuilder<...> AddSortBy<TImplementation>()
        where TImplementation : class, ISortedQueryBuilder<TEntity, TKey, TSortBy>;

    ComplexEntityServiceBuilder<...> SortBy(
        Func<IQueryable<TEntity>, TSortBy?, IQueryable<TEntity>> sortByFunc);

    // NEW: typed includes
    ComplexEntityServiceBuilder<...> Includes<TImplementation>()
        where TImplementation : class, IIncludableQueryBuilder<TEntity, TKey, TIncludes>;

    ComplexEntityServiceBuilder<...> Includes(
        Func<IQueryable<TEntity>, TIncludes?, IQueryable<TEntity>> addIncludes);

    // NEW: typed processors
    ComplexEntityServiceBuilder<...> Process(Func<IList<TEntity>, TIncludes?, Task> process);
    ComplexEntityServiceBuilder<...> Process(Action<TEntity, TIncludes?> process);
    ComplexEntityServiceBuilder<...> AddProcessor<TImplementation>()
        where TImplementation : class, IEntityProcessor<TEntity, TIncludes>;

    void Build();
}
```

---

### ComplexEntityIntServiceBuilder

Returned by `.For<TEntity, TSearchObject, TSortBy, TIncludes>()`.
Inherits all `ComplexEntityServiceBuilder` methods. Only addition vs parent:

```csharp
public partial class ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes>
    : ComplexEntityServiceBuilder<TContext, TEntity, int, TSearchObject, TSortBy, TIncludes>
{
    // Int-key shortcut — no TRelatedKey type parameter needed
    ComplexEntityIntServiceBuilder<...> Related<TRelated>(
        Expression<Func<TEntity, ICollection<TRelated>?>> navigationExpression,
        Action<TEntity>? prepareFunc = null)
        where TRelated : class, IEntity<int>;

    void Build();
}
```

---

### MappedEntityServiceBuilder

Returned by `UseMapping<TDto, TInputDto>()`. Inherits all builder methods.

```csharp
// Base variant — any source/target after-mapper
public class MappedEntityServiceBuilder<TContext, TEntity, TKey>
    : EntityServiceBuilder<TContext, TEntity, TKey>
{
    MappedEntityServiceBuilder<TContext, TEntity, TKey> After<TImplementation>()
        where TImplementation : class, IEntityAfterMapper;

    MappedEntityServiceBuilder<TContext, TEntity, TKey> After<TImplementation>(
        Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityAfterMapper;

    MappedEntityServiceBuilder<TContext, TEntity, TKey> After<TSource, TTarget>(
        Action<TSource, TTarget> afterMapAction);

    MappedEntityServiceBuilder<TContext, TEntity, TKey> After<TSource, TTarget>(
        Func<IServiceProvider, Action<TSource, TTarget>> afterMapActionFactory);
}

// Typed variant — TDto and TInputDto known; shortcut After/AfterInput
public class MappedEntityServiceBuilder<TContext, TEntity, TKey, TDto, TInputDto>
    : MappedEntityServiceBuilder<TContext, TEntity, TKey>
{
    // Entity → TDto after-mapper
    MappedEntityServiceBuilder<TContext, TEntity, TKey, TDto, TInputDto> After(
        Action<TEntity, TDto> afterMapAction);

    // TInputDto → Entity after-mapper
    MappedEntityServiceBuilder<TContext, TEntity, TKey, TDto, TInputDto> AfterInput(
        Action<TInputDto, TEntity> afterMapAction);
}
```

---

## Mapping and Processing

### IEntityMapper

```csharp
using Regira.Entities.Mapping.Abstractions;

public interface IEntityMapper
{
    TTarget Map<TTarget>(object source);
    TTarget Map<TSource, TTarget>(TSource source, TTarget target);
}
```

### After Mappers

```csharp
using Regira.Entities.Mapping.Abstractions;

public interface IEntityAfterMapper
{
    bool CanMap(object source);
    void AfterMap(object source, object target);
}

public interface IEntityAfterMapper<in TSource, in TTarget> : IEntityAfterMapper
{
    void AfterMap(TSource source, TTarget target);
}

// Inherit to create a custom after-mapper class
public abstract class EntityAfterMapperBase<TSource, TTarget> : IEntityAfterMapper<TSource, TTarget>
{
    public abstract void AfterMap(TSource source, TTarget target);
    public bool CanMap(object source);
}
```

### Query Builders

```csharp
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

public interface IFilteredQueryBuilder<TEntity, TKey, in TSearchObject>
    where TSearchObject : ISearchObject<TKey>
{
    IQueryable<TEntity> Build(IQueryable<TEntity> query, TSearchObject? so);
}

// Preferred base class — inherit and override Build().
public abstract class FilteredQueryBuilderBase<TEntity>
    : FilteredQueryBuilderBase<TEntity, SearchObject<int>>
    where TEntity : IEntity<int>;

public abstract class FilteredQueryBuilderBase<TEntity, TSearchObject>
    : FilteredQueryBuilderBase<TEntity, int, TSearchObject>
    where TEntity : IEntity<int>
    where TSearchObject : ISearchObject<int>;

public abstract class FilteredQueryBuilderBase<TEntity, TKey, TSearchObject>
    : IFilteredQueryBuilder<TEntity, TKey, TSearchObject>
    where TSearchObject : ISearchObject<TKey>
{
    public abstract IQueryable<TEntity> Build(IQueryable<TEntity> query, TSearchObject? so);
}

// 2-param shortcut defaults TSortBy to EntitySortBy
public interface ISortedQueryBuilder<TEntity, TKey> : ISortedQueryBuilder<TEntity, TKey, EntitySortBy>
    where TEntity : IEntity<TKey>;

public interface ISortedQueryBuilder<TEntity, TKey, TSortBy>
    where TEntity : IEntity<TKey>
    where TSortBy : struct, Enum
{
    IQueryable<TEntity> SortBy(IQueryable<TEntity> query, TSortBy? sortBy = null);
}

// 2-param shortcut defaults TIncludes to EntityIncludes
public interface IIncludableQueryBuilder<TEntity, TKey> : IIncludableQueryBuilder<TEntity, TKey, EntityIncludes>
    where TEntity : IEntity<TKey>;

public interface IIncludableQueryBuilder<TEntity, TKey, TIncludes>
    where TEntity : IEntity<TKey>
    where TIncludes : struct, Enum
{
    IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query, TIncludes? includes = null);
}
```

### Processors

```csharp
using Regira.Entities.EFcore.Processing.Abstractions;

public interface IEntityProcessor<TEntity, in TIncludes>
    where TIncludes : struct, Enum
{
    Task Process(IList<TEntity> items, TIncludes? includes, CancellationToken token = default);
}
```

### Preppers

```csharp
using Regira.Entities.EFcore.Preppers.Abstractions;

public interface IEntityPrepper
{
    Task Prepare(object modified, object? original, CancellationToken token = default);
}

public interface IEntityPrepper<in TEntity> : IEntityPrepper
{
    Task Prepare(TEntity modified, TEntity? original, CancellationToken token = default);
}
```

### Primers

```csharp
using Regira.Entities.EFcore.Primers.Abstractions;

public interface IEntityPrimer
{
    Task PrepareManyAsync(IList<EntityEntry> entries, CancellationToken token = default);
    Task PrepareAsync(object entity, EntityEntry entry, CancellationToken token = default);
    bool CanPrepare(object entity);
}

public interface IEntityPrimer<in T> : IEntityPrimer
{
    Task PrepareAsync(T entity, EntityEntry entry, CancellationToken token = default);
    bool CanPrepare(T entity);
}
```

### Normalizers

```csharp
using Regira.Entities.EFcore.Normalizing.Abstractions;

public interface IEntityNormalizer
{
    bool IsExclusive { get; }
    Task HandleNormalize(object item, CancellationToken token = default);
    Task HandleNormalizeMany(IEnumerable<object> items, CancellationToken token = default);
}

public interface IEntityNormalizer<in T> : IEntityNormalizer
{
    Task HandleNormalize(T item, CancellationToken token = default);
    Task HandleNormalizeMany(IEnumerable<T> items, CancellationToken token = default);
}
```

---

## Response Types

```csharp
using Regira.Entities.Web.Models;

public class DetailsResult<TDto>  { public TDto Item { get; set; }        public long? Duration { get; set; } }
public class ListResult<TDto>     { public IList<TDto> Items { get; set; } public long? Duration { get; set; } }
public class SearchResult<TDto>   { public IList<TDto> Items { get; set; } public long Count { get; set; }     public long? Duration { get; set; } }
public class SaveResult<TDto>     { public TDto Item { get; set; }        public bool IsNew { get; set; }     public int Affected { get; set; }   public long? Duration { get; set; } }
public class DeleteResult<TDto>   { public TDto Item { get; set; }        public long? Duration { get; set; } }
```

---

## Attachments

```csharp
using Regira.Entities.Attachments.Abstractions;

public interface IAttachment<TKey>
{
    TKey Id { get; set; }
    string? Filename { get; set; }
    string? Uri { get; set; }
    long? Size { get; set; }
}

public interface IEntityAttachment;

public interface IEntityAttachment<TKey, TObjectKey, TAttachmentKey, TAttachment>
    : IEntityAttachment, IEntity<TKey>, IHasObjectId<TObjectKey>
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
{
    TAttachmentKey AttachmentId { get; set; }
    TAttachment? Attachment { get; set; }
}
```

### Attachment Controller

```csharp
using Regira.Entities.Web.Attachments.Abstractions;

// Simplest variant
public abstract class EntityAttachmentControllerBase<TEntity>
    : EntityAttachmentControllerBase<TEntity, EntityAttachmentDto, EntityAttachmentInputDto>
    where TEntity : class, IEntityAttachment<int, int, int, Attachment>, IEntity<int>;

// Standard variant — route pattern: api/{entity}/{objectId}/attachments
public abstract class EntityAttachmentControllerBase<TEntity, TDto, TInputDto>
    : ControllerBase
    where TEntity : class, IEntityAttachment<int, int, int, Attachment>, IEntity<int>
    where TInputDto : class, IEntityAttachmentInput;
```

---

## Exceptions

```csharp
using Regira.Entities.Models;

// Throw to return HTTP 400 from a controller action
public class EntityInputException<T>(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public T? Item { get; set; }
    public IDictionary<string, string> InputErrors { get; set; }
}
```

---

## Supporting Types

```csharp
using Regira.DAL.Paging;

public class PagingInfo
{
    public int PageSize { get; set; }
    public int Page { get; set; } = 1;
}
```

```csharp
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;

public class EntityServiceCollectionOptions(IServiceCollection services)
{
    public IServiceCollection Services { get; }
}
```

---

## See Also

- [Entities Instructions](./entities.instructions.md) — Complete framework guide and decision rules
- [Entities Examples](./entities.examples.md) — Working code patterns
- [Entities Namespaces](./entities.namespaces.md) — Full namespace listing

