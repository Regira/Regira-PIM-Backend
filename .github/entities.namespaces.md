# Regira Entities — Namespace Reference

> **AI Agent Rule**: You MUST use the exact namespaces listed in this file.
> You are NOT allowed to guess, invent, or assume any namespace.
> If a type is not listed here, look it up in the codebase before using it.

---

## Entity Interfaces & Base Models

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.Models` | `SearchObject<>`, `EntitySortBy`, `EntityInputException<>`, `EntityIncludes` |
| `Regira.Entities.Models.Abstractions` | `IEntity<>`, `IEntityWithSerial`, `ISearchObject<>`, `IHasTimestamps`, `IHasCreated`, `IHasLastModified`, `IHasTitle`, `IHasNormalizedTitle`, `IHasDescription`, `IHasCode`, `IHasNormalizedContent`, `IHasLastNormalized`, `IArchivable`, `ISortable`, `IHasObjectId<>`, `IHasAggregateKey`, `IHasDefault<>`, `IHasParentEntity<>`, `IHasPassword`, `IHasEncryptedPassword`, `IHasSlug`, `IHasStartDate`, `IHasEndDate`, `IHasStartEndDate`, `IHasUri`, `IHasUserId` |

---

## Services

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.Services.Abstractions` | `IEntityService<>`, `IEntityReadService<>`, `IEntityWriteService<>`, `IEntityRepository<>`, `IEntityManager<>`, `EntityWrappingServiceBase<>` |
| `Regira.Entities.Services` | `EntityManager<>` |
| `Regira.Entities.EFcore.Services` | `EntityRepository<>`, `EntityReadService<>`, `EntityWriteService<>` |

---

## Query Builders

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.EFcore.QueryBuilders.Abstractions` | `IQueryBuilder<>`, `IFilteredQueryBuilder<>`, `IGlobalFilteredQueryBuilder<>`, `ISortedQueryBuilder<>`, `IIncludableQueryBuilder<>`, `FilteredQueryBuilderBase<>`, `GlobalFilteredQueryBuilderBase<>` |
| `Regira.Entities.EFcore.QueryBuilders` | `QueryBuilder<>`, `EntityQueryFilter<>`, `SortedQueryBuilder<>`, `IncludableQueryBuilder<>` |
| `Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders` | `FilterIdsQueryBuilder`, `FilterArchivablesQueryBuilder`, `FilterHasCreatedQueryBuilder`, `FilterHasLastModifiedQueryBuilder`, `FilterHasNormalizedContentQueryBuilder` |
| `Regira.Entities.EFcore.Extensions` | `QueryExtensions` *(EF Core LINQ helpers: `FilterId`, `FilterIds`, `FilterExclude`, `FilterCode`, `FilterTitle`, `FilterNormalizedTitle`, `FilterCreated`, `FilterLastModified`, `FilterTimestamps`, `FilterQ`, `FilterArchivable`, `FilterHasAttachment`, `SortQuery`)* |
| `Regira.DAL.Paging` | `QueryExtensions` *(`PageQuery`)* |

---

## Processors

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.EFcore.Processing.Abstractions` | `IEntityProcessor<>` |
| `Regira.Entities.EFcore.Processing` | `EntityProcessor<>` |

---

## Preppers

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.EFcore.Preppers.Abstractions` | `IEntityPrepper<>`, `EntityPrepperBase<>` |
| `Regira.Entities.EFcore.Preppers` | `EntityPrepper<>`, `RelatedCollectionPrepper<>` |

---

## Primers (EF Core SaveChanges Interceptors)

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.EFcore.Primers` | `ArchivablePrimer`, `HasCreatedDbPrimer`, `HasLastModifiedDbPrimer`, `AutoTruncatePrimer`, `AutoNormalizingPrimer`, `AddPrimerInterceptors(IServiceProvider)` *(DbContextOptionsBuilder extension)* |
| `Regira.Entities.EFcore.Primers.Abstractions` | `IEntityPrimer<>`, `EntityPrimerBase<>` |
| `Regira.Entities.DependencyInjection.Primers` | `ServiceCollectionPrimerExtensions` *(`AddPrimer<>()`, `AddDefaultPrimers()`, `AddAutoTruncatePrimer()`, `AddDefaultEntityNormalizerPrimer()` — on `IServiceCollection` and `EntityServiceCollectionOptions`)* |

---

## Normalizing

| Namespace | Types |
|-----------|-------|
| `Regira.Normalizing` | `[NormalizedAttribute]`, `ObjectNormalizer`, `DefaultNormalizer`, `NormalizingDefaults` |
| `Regira.Normalizing.Abstractions` | `INormalizer`, `IObjectNormalizer` |
| `Regira.Normalizing.Models` | `NormalizingOptions` |
| `Regira.Entities.EFcore.Normalizing` | `DefaultEntityNormalizer`, `AddNormalizerInterceptors(IServiceProvider)` *(DbContextOptionsBuilder extension)* |
| `Regira.Entities.EFcore.Normalizing.Abstractions` | `IEntityNormalizer<>`, `EntityNormalizerBase<>`, `EntityNormalizingOptions` |
| `Regira.Entities.DependencyInjection.Normalizers` | `ServiceCollectionNormalizerExtensions` *(`AddNormalizer<>()`, `AddDefaultEntityNormalizer()`, `AddDefaultQKeywordHelper()` — on `IServiceCollection` and `EntityServiceCollectionOptions`)*, `EntityDefaultNormalizingOptions` |
| `Regira.Entities.Keywords` | `QKeywordHelper`, `QKeywordHelperOptions`, `QKeyword`, `ParsedKeywordCollection` |
| `Regira.Entities.Keywords.Abstractions` | `IQKeywordHelper` |

---

## Dependency Injection

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.DependencyInjection.ServiceBuilders.Extensions` | `ServiceCollectionExtensions` *(`UseEntities<TContext>()` — on `IServiceCollection`)*, `EntityServiceCollectionExtensions` *(`UseDefaults()` — on `EntityServiceCollectionOptions`)* |
| `Regira.Entities.DependencyInjection.ServiceBuilders` | `EntityServiceCollection<>`, `EntityServiceBuilder<>`, `EntityIntServiceBuilder<>`, `EntitySearchObjectServiceBuilder<>`, `ComplexEntityServiceBuilder<>`, `ComplexEntityIntServiceBuilder<>` |
| `Regira.Entities.DependencyInjection.ServiceBuilders.Models` | `EntityServiceCollectionOptions` |
| `Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions` | `IEntityServiceCollection<>` |
| `Regira.Entities.DependencyInjection.QueryBuilders` | `ServiceCollectionQueryFilterExtensions` *(`AddQueryFilter<>()`, `AddGlobalFilterQueryBuilder<>()`, `RemoveGlobalQueryFilters()`, `AddDefaultGlobalQueryFilters()` — on `IServiceCollection` and `EntityServiceCollectionOptions`)* |
| `Regira.Entities.DependencyInjection.Preppers` | `ServiceCollectionPrepperExtensions` *(`AddPrepper<>()` — on `IServiceCollection` and `EntityServiceCollectionOptions`)* |
| `Regira.Entities.DependencyInjection.Mapping` | `ServiceCollectionMappingExtensions` *(`AddMapping<>()`, `AddAfterMapper<>()`, `AfterMap<>()` — on `EntityServiceCollectionOptions`)*, `MappedEntityServiceBuilder<>` |

---

## Mapping

| Namespace | Types | Notes |
|-----------|-------|-------|
| `Regira.Entities.Mapping.Mapster` | `UseMapsterMapping()` | **Default** mapping provider |
| `Regira.Entities.Mapping.AutoMapper` | `UseAutoMapper()` | Alternative mapping provider |
| `Regira.Entities.Mapping.Abstractions` | `IEntityMapper`, `IEntityAfterMapper<>`, `IEntityMapConfigurator`, `EntityAfterMapperBase<>`, `EntityAfterMapper<>` | |
| `Regira.Entities.Mapping.Models` | `AttachmentDto<>`, `AttachmentInputDto<>`, `EntityAttachmentDto<>`, `EntityAttachmentInputDto<>` | DTO classes for mapping |
| `Regira.Entities.Attachments.Mapping.Abstractions` | `IEntityAttachmentInput<>` | Attachment input contracts |

---

## EF Core Extensions & Interceptors

| Namespace | Types |
|-----------|-------|
| `Regira.DAL.EFcore.Extensions` | `SetDecimalPrecisionConvention(int precision, int scale)` *(ModelBuilder extension)* |
| `Regira.DAL.EFcore.Services` | `AddAutoTruncateInterceptors()` *(DbContextOptionsBuilder extension)* |
| `Regira.DAL.Paging` | `PagingInfo` |

---

## Attachments

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.Attachments.Abstractions` | `IAttachment<>`, `IEntityAttachment<>`, `IHasAttachments<>`, `IAttachmentService<>`, `IAttachmentFileService<>`, `IAttachmentSearchObject<>`, `IEntityAttachmentSearchObject<>`, `IAttachmentUriResolver<>`, `IFileIdentifierGenerator` |
| `Regira.Entities.Attachments.Models` | `Attachment<>`, `EntityAttachment<>`, `AttachmentSearchObject<>`, `EntityAttachmentSearchObject<>` |
| `Regira.Entities.Attachments` | `EntityAttachmentUriAfterMapper<>` |
| `Regira.Entities.EFcore.Attachments` | `ITypedAttachmentService`, `TypedAttachmentService<>`, `AttachmentFilteredQueryBuilder<>`, `EntityAttachmentFilteredQueryBuilder<>`, `AttachmentProcessor<>`, `EntityAttachmentProcessor<>`, `AttachmentPrimer`, `EntityAttachmentPrimer`, `DefaultFileIdentifierGenerator<>` |
| `Regira.Entities.DependencyInjection.Attachments` | `EntityAttachmentServiceBuilder<>`, `EntityServiceBuilderExtensions` *(`HasAttachments<>()` — on `EntityServiceBuilder<>`)*, `IEntityAttachmentServiceBuilder<>` |
| `Regira.Entities.Web.Attachments.Abstractions` | `EntityAttachmentControllerBase<>` |
| `Regira.IO.Storage.Abstractions` | `IFileService` |
| `Regira.IO.Storage.FileSystem` | `BinaryFileService`, `FileSystemOptions` |
| `Regira.IO.Storage.Azure` | `BinaryBlobService`, `AzureOptions`, `AzureCommunicator` |
| `Regira.IO.Storage.SSH` | `SftpService`, `SftpCommunicator`, `SftpConfig` |

---

## Extensions

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.Extensions` | `EntityExtensions` *(`IsNew<TKey>()` — on `IEntity<TKey>`; `SetSortOrder()` — on `IEnumerable<ISortable>`)* |

---

## Controllers

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.Web.Controllers.Abstractions` | `EntityControllerBase<>` |
| `Microsoft.AspNetCore.Mvc` | `[ApiController]`, `[Route]`, `ControllerBase` |

---

## Common .NET / EF Core Namespaces

| Namespace | Types |
|-----------|-------|
| `System.ComponentModel.DataAnnotations` | `[Required]`, `[MaxLength]`, `[Range]` |
| `Microsoft.EntityFrameworkCore` | `DbContext`, `DbSet<T>`, `ModelBuilder`, `EntityState`, `EF.Functions.Like(...)`, `Include(...)`, `ThenInclude(...)`, `OrderBy(...)` |
| `Microsoft.EntityFrameworkCore.ChangeTracking` | `EntityEntry` |
| `Microsoft.Extensions.DependencyInjection` | `IServiceCollection`, `IServiceProvider` |

---

## Grouped by Use Case (Quick Lookup)

### Setting up a new project
```
Regira.Entities.DependencyInjection.ServiceBuilders.Extensions   → UseEntities<TContext>() (on IServiceCollection)
Regira.Entities.DependencyInjection.ServiceBuilders.Extensions   → UseDefaults() (on EntityServiceCollectionOptions)
Regira.Entities.Mapping.Mapster                                  → UseMapsterMapping()
Regira.Entities.EFcore.Primers                                   → AddPrimerInterceptors(sp)
Regira.Entities.EFcore.Normalizing                               → AddNormalizerInterceptors(sp)
Regira.DAL.EFcore.Services                                       → AddAutoTruncateInterceptors()
Regira.DAL.EFcore.Extensions                                     → SetDecimalPrecisionConvention()
```

### Creating an entity
```
Regira.Entities.Models.Abstractions      → IEntityWithSerial, IHasTimestamps, IHasTitle,
                                           IHasDescription, IHasCode, IArchivable,
                                           ISortable, IHasNormalizedContent
Regira.Entities.Attachments.Abstractions → IHasAttachments
Regira.Normalizing                       → [NormalizedAttribute]
System.ComponentModel.DataAnnotations    → [Required], [MaxLength], [Range]
```

### Creating SearchObject / SortBy / Includes
```
Regira.Entities.Models   → SearchObject<>, EntityIncludes
```
*(SortBy is a plain enum — no using required. `EntityIncludes` is the base `[Flags]` enum; extend it for project-specific Includes enums.)*

### Full-text search (Q / keywords)
```
Regira.Entities.Keywords            → QKeyword, ParsedKeywordCollection, QKeywordHelper
Regira.Entities.Keywords.Abstractions → IQKeywordHelper
Regira.Entities.EFcore.Extensions   → QueryExtensions (FilterQ)
```

### Building a query builder
```
Regira.Entities.EFcore.QueryBuilders.Abstractions   → IFilteredQueryBuilder<TEntity, TKey, TSearchObject>
                                                       ISortedQueryBuilder<TEntity, TKey, TSortBy>
                                                       IIncludableQueryBuilder<TEntity, TKey, TIncludes>
                                                       FilteredQueryBuilderBase<TEntity, TKey, TSearchObject>
                                                       GlobalFilteredQueryBuilderBase<TEntity, TKey>
Regira.Entities.EFcore.Extensions                  → QueryExtensions (FilterId, FilterQ, etc.)
Regira.DAL.Paging                                  → QueryExtensions (PageQuery)
Regira.Entities.Keywords.Abstractions              → IQKeywordHelper
Microsoft.EntityFrameworkCore                      → EF.Functions.Like(...)
```

### Creating a processor
```
Regira.Entities.EFcore.Processing.Abstractions   → IEntityProcessor<TEntity, TIncludes>
```

### Creating a prepper
```
Regira.Entities.EFcore.Preppers.Abstractions   → EntityPrepperBase<TEntity>, IEntityPrepper<TEntity>
```

### Creating a primer
```
Regira.Entities.EFcore.Primers.Abstractions   → EntityPrimerBase<T>, IEntityPrimer<T>
Microsoft.EntityFrameworkCore.ChangeTracking  → EntityEntry
Microsoft.EntityFrameworkCore                 → EntityState
```

### Creating a normalizer
```
Regira.Entities.EFcore.Normalizing.Abstractions   → EntityNormalizerBase<T>, IEntityNormalizer<T>
Regira.Normalizing.Abstractions                   → INormalizer
Regira.Normalizing.Models                         → NormalizingOptions
```

### Creating a wrapping service
```
Regira.Entities.Services.Abstractions   → EntityWrappingServiceBase<...>, IEntityService<...>
Regira.Entities.Models                  → EntityInputException<T>
Regira.DAL.Paging                       → PagingInfo
```

### Creating a controller
```
Regira.Entities.Web.Controllers.Abstractions   → EntityControllerBase<...>
Microsoft.AspNetCore.Mvc                       → [ApiController], [Route]
```

### Registering an entity in DI
```
Regira.Entities.DependencyInjection.ServiceBuilders              → EntityServiceCollection<TContext> (returned by UseEntities())
Regira.Entities.DependencyInjection.ServiceBuilders.Models       → EntityServiceCollectionOptions
Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions → IEntityServiceCollection<TContext>
Microsoft.EntityFrameworkCore                                    → DbContext (constraint)
```

### Setting up attachments
```
Regira.Entities.Attachments.Abstractions            → IHasAttachments, IEntityAttachment<TKey, TObjectKey>
Regira.Entities.Attachments.Models                  → EntityAttachment, EntityAttachment<TKey, TObjectKey, TAttachmentKey, TAttachment>, Attachment, Attachment<TKey>
Regira.Entities.Mapping.Models                      → EntityAttachmentDto, EntityAttachmentInputDto
Regira.Entities.Web.Attachments.Abstractions        → EntityAttachmentControllerBase<...>
Regira.IO.Storage.FileSystem                        → BinaryFileService, FileSystemOptions
Regira.IO.Storage.Azure                             → BinaryBlobService, AzureOptions, AzureCommunicator
Regira.IO.Storage.SSH                               → SftpService, SftpCommunicator, SftpConfig
Regira.IO.Storage.Abstractions                      → IFileService
```
