# PIM Backend

A sample **Product Information Management** API built on top of the [Regira Entities](https://regira.github.io/Regira-Codebase/src/Common.Entities/) framework.

The project is intentionally kept close to real-world complexity — it shows how Regira Entities handles filtering, sorting, includes, DTO mapping, tree structures, and DI wiring — without hiding anything behind project-specific magic.

The sample dataset is a **worldwide recipe book**: ~195 countries, hundreds of ingredients, and traditional dishes seeded from CSV files. It could just as well be a product catalogue, a parts inventory, or a digital library. The domain is flexible by design.

A live instance is running, backed by a separate front-end app.

| Site | URL |
|---|---|
| 🏢 Regira | [regira.com](https://www.regira.com) |
| 📚 Regira Entities | [Regira Entities framework](https://regira.github.io/Regira-Codebase/src/Common.Entities/) |
| 🌐 Live demo | [pim.regira.com/manager](https://pim.regira.com/manager/) |
| 💻 Front-end app | [Regira/Regira-PIM-Admin](https://github.com/Regira/Regira-PIM-Admin) |

> ⚠️ **A valid license for Regira Entities is required to run this example.** You can request one — including a free trial — at [regira.com/licensing?product=regira.entities](https://regira.com/licensing?product=regira.entities#request). See [License](#license) for details.

---

## Stack

| Layer | Technology |
|---|---|
| Framework | .NET 10 / ASP.NET Core |
| ORM | [Entity Framework Core](https://www.nuget.org/packages/microsoft.entityframeworkcore/) (SQL Server or SQLite) |
| DTO mapping | [Mapster](https://www.nuget.org/packages/Mapster/) via `Regira.Entities.Mapping.Mapster` |
| Tree queries | [Regira.TreeList](https://regira.github.io/Regira-Codebase/src/TreeList/) |
| Authentication | JWT Bearer via `Regira.Security.Authentication.Web` |
| Email | [MailGun](https://www.mailgun.com/) via `Regira.Office.Mail.MailGun` |
| API docs | OpenAPI + [Scalar](https://www.nuget.org/packages/Scalar.AspNetCore) |
| Logging | [Serilog](https://www.nuget.org/packages/Serilog) |

---

## Project layout

```
app/
  PIM.Manager.API/          ← the runnable API
  PIM.DataGenerator/        ← CSV seeding console app
  PIM.EFConsole/            ← EF Core migration helper
  PIM.AccountDataGenerator/ ← user/account seeding

src/
  PIM.Core/                 ← constants, interfaces, claim types
  PIM.Models/               ← domain entities + DTOs
  PIM.Data/                 ← DbContext, EF configuration, DB functions
  PIM.Services/             ← repositories, query builders, normalizers
  PIM.Web/                  ← API controllers
  PIM.DependencyInjection/  ← AddPimServices(), AddAdminServices()
  PIM.Identity/             ← users, roles, JWT claims
  PIM.Admin/                ← admin-facing service layer
```

---

## Domain model

The three core domains — **Catalog**, **Taxonomy**, and **Stakeholders** — are intentionally generic. The seed data frames them as a recipe book (ingredients, dishes, cuisines, dietary tags), but the same structure works equally well for a hardware catalogue, a pharmaceutical inventory, a media library, or anything else that needs composable items, hierarchical categories, and related parties.

### Catalog — Products

A `Product` is anything that can be identified, measured, and composed. In the sample data that means ingredients and recipes; in another context it could be components and assemblies, SKUs and bundles, or tracks and albums.

```csharp
public class Product : IEntityWithSerial, IHasCode, IHasTitle, IHasDescription,
    IHasNormalizedTitle, IHasNormalizedContent, IHasTimestamps, IArchivable
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    // measurement
    public int? UnitTypeId { get; set; }
    public UnitType? UnitType { get; set; }
    public decimal? Quantity { get; set; }

    // navigation
    public ICollection<ProductFacet>? Facets { get; set; }
    public ICollection<ProductComponent>? Components { get; set; }   // ingredients
    public ICollection<ProductComponent>? Assemblies { get; set; }   // recipes using this
    public ICollection<ProductSupplier>? Suppliers { get; set; }
}
```

`ProductComponent` is a self-referential join that lets recipes reference their ingredients — which may themselves be assembled from sub-ingredients (stocks, spice mixes, etc.).

### Taxonomy — Facets

`Facet` is a hierarchical tag node. Products receive facets through `ProductFacet`, and facets are grouped semantically via `FacetGroup`:

```
FOOD_TYPES  →  SOUPS, PASTA, GRILLED, BREADS, DESSERTS …
PROTEINS    →  POULTRY, BEEF, PORK, LAMB, SEAFOOD …
DIETARY     →  VEGAN, VEGETARIAN, GLUTEN_FREE, KETO …
ALLERGENS   →  GLUTEN, DAIRY, EGGS, SHELLFISH, PEANUTS …
```

A product can carry many facets, making it straightforward to query "all vegan soups that are nut-free".

### Stakeholders — Parties

`Party` is an abstract base discriminated into `Person` and `Organization`. Parties can be linked to products as suppliers and to each other through `PartyRelationship` (parent–child hierarchies for things like manufacturer → distributor → retailer).

---

## How Regira Entities is used

> **Note:** what follows is a customised, multi-domain setup. The framework default is considerably simpler — a single `UseEntities()` call with a few `.For<>()` blocks is enough to get a fully functional CRUD API. If you are working with an AI coding assistant, the Regira MCP server exposes a `get_example` tool that returns ready-to-use minimal examples for any supported package.

### 1. Service registration

`UseEntities()` sets up global defaults and the Mapster mapping layer. Each domain then adds its own services through a dedicated extension method, keeping things organised as the project grows:

```csharp
// src/PIM.DependencyInjection/Extensions/ServiceCollectionExtensions.cs
services
    .UseEntities<PimDbContext>(options =>
    {
        options.UseDefaults();
        options.UseMapsterMapping(cfg =>
        {
            // polymorphic Party → PersonDto / OrganizationDto at mapping time
            cfg.ForType<Party, PartyDto>()
                .MapWith(src => (src as Person) != null
                    ? (src as Person).Adapt<PersonDto>()!
                    : (src as Organization).Adapt<OrganizationDto>()!);
        });
    })
    .AddCatalog()       // Products, UnitTypes
    .AddStakeholders()  // Parties, RelationshipTypes
    .AddCountries();
```

Inside each domain extension the `.For<>()` builder wires up filters, sorting, includes, related collections, and the repository — all in one place:

```csharp
// src/PIM.DependencyInjection/Catalog/ProductServiceConfiguration.cs
services.For<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(e =>
{
    e.AddFilter<ProductQueryFilter>();
    e.AddSortBy<ProductSortingQueryBuilder>();
    e.AddIncludes<ProductIncludingQueryBuilder>();

    e.Related(item => item.Facets);
    e.Related(item => item.Components, item => item.Components?.SetSortOrder());
    e.Related(item => item.Suppliers);

    e.AddNormalizer<ProductNormalizer>();
    e.HasRepository<ProductRepository>();
    e.AddTransient<IProductRepository, ProductRepository>();
    e.AddTransient<IProductService, ProductValidateManager>();
    e.UseEntityService<ProductValidateManager>();
});
```

### 2. Filtering via SearchObject

Each entity has a typed `SearchObject` that maps cleanly to query-builder logic:

```csharp
public class ProductSearchObject : SearchObject
{
    public string? Q { get; set; }               // full-text across title + content
    public ICollection<int>? FacetId { get; set; }
    public ICollection<string>? FacetCode { get; set; }
    public int? UnitTypeId { get; set; }
    public bool? HasComponents { get; set; }
    public bool? HasAssemblies { get; set; }
}
```

A `GET /products?facetCode=VEGAN&facetCode=GLUTEN_FREE&q=pasta` is all it takes.

### 3. Controllers

Controllers inherit `EntityControllerBase` and get CRUD + search for free. Custom tree-traversal actions are added on top:

```csharp
[ApiController, Route("products")]
public class ProductController
    : EntityControllerBase<Product, ProductSearchObject, ProductSortBy,
                           ProductIncludes, ProductDto, ProductInputDto>
{
    private readonly IProductService _productService;

    [HttpGet("{id}/ancestors")]
    public Task<IEnumerable<ProductDto>> GetAncestors(int id)
        => _productService.GetAncestors(id);

    [HttpGet("{id}/offspring")]
    public Task<IEnumerable<ProductDto>> GetOffspring(int id)
        => _productService.GetOffspring(id);

    [HttpGet("{id}/family")]
    public Task<IEnumerable<ProductDto>> GetFamily(int id)
        => _productService.GetFamily(id);
}
```

### 4. DTO mapping with Mapster

Polymorphic Party types are resolved at mapping time so the API always returns the right shape:

```csharp
TypeAdapterConfig.GlobalSettings.NewConfig<Party, PartyDto>()
    .ConstructUsing(src => src is Person
        ? new PersonDto()
        : new OrganizationDto());
```

### 5. EF Core interceptors (Primer, Normalizer, AutoTruncate)

Three interceptors run transparently on every `SaveChanges`:

- **Primer** — stamps `Created` / `LastModified` timestamps
- **Normalizer** — fills `NormalizedContent` fields for full-text search (declared with `[Normalized]`)
- **AutoTruncate** — silently trims values that exceed the column `[MaxLength]`

---

## Seed data

Run `PIM.DataGenerator` to populate an empty database. It reads five CSV files from `app/PIM.DataGenerator/Assets/`:

| File | Contents |
|---|---|
| `unit-types.csv` | 16 measurement units (g, kg, mL, cup, sprig, clove …) |
| `facets.csv` | 50+ hierarchical facet nodes |
| `facet-groups.csv` | 9 semantic groups (DIETARY, ALLERGENS, PROTEINS …) |
| `ingredients.csv` | 1 000+ canonical ingredients with unit and facet assignments |
| `recipes-per-country.csv` | ~195 countries × 3–5 traditional dishes each |

Seeding runs in phases so that component references (e.g. "Chicken Stock" used inside "Tom Kha Gai") resolve correctly.

---

## Running the API

```bash
# restore & run (SQLite by default)
dotnet run --project app/PIM.Manager.API

# open the interactive docs
# http://localhost:<port>/scalar
```

> ⚠️ **A license for Regira Entities is required to run this example.** Set `Regira:LicenseKey` in `appsettings.json` (or as an environment variable). You can request a license — including a free trial — at [regira.com/licensing?product=regira.entities](https://regira.com/licensing?product=regira.entities#request).

Switch to SQL Server by replacing the `ConnectionStrings:PimDb` value and the EF provider in `appsettings.json`.

---

## Authorization

Two claims-based roles gate the API:

| Policy | Access |
|---|---|
| `AdminOrEditor` | Default — read/write most entities |
| `EditorOnly` | Write access to catalogue data |
| `AdminOnly` | User management |

Obtain a token via `POST /account/login`. The `AccountsDbContext` runs in a separate database from the PIM data.

---

## Related packages

| Package | Role in this project |
|---|---|
| [`Regira.Entities`](https://www.nuget.org/packages/Regira.Entities) | Entity abstractions, service interfaces |
| [`Regira.Entities.EFcore`](https://www.nuget.org/packages/Regira.Entities.EFcore) | `EntityRepository`, EF interceptors |
| [`Regira.Entities.DependencyInjection`](https://www.nuget.org/packages/Regira.Entities.DependencyInjection) | `UseEntities()` / `.For<>()` builder |
| [`Regira.Entities.Mapping.Mapster`](https://www.nuget.org/packages/Regira.Entities.Mapping.Mapster) | Mapster DTO pipeline integration |
| [`Regira.Entities.Web.Controllers.Abstractions`](https://www.nuget.org/packages/Regira.Entities.Web.Controllers.Abstractions) | `EntityControllerBase` |
| [`Regira.TreeList`](https://www.nuget.org/packages/Regira.TreeList) | Ancestor / offspring / family tree queries |
| [`Regira.DAL.EFcore.Services`](https://www.nuget.org/packages/Regira.DAL.EFcore.Services) | Low-level data-access helpers |
| [`Regira.Security.Authentication.Web`](https://www.nuget.org/packages/Regira.Security.Authentication.Web) | JWT bearer setup |
| [`Regira.Office.Mail.MailGun`](https://www.nuget.org/packages/Regira.Office.Mail.MailGun) | Transactional email |

---

## License

This example is built on the **Regira Entities** framework, which requires a valid license to run. Without a license key the API will not start (unless validation is explicitly skipped during local development).

You can request a license — including a **free trial** — here:

➡️ **[regira.com/licensing?product=regira.entities](https://regira.com/licensing?product=regira.entities#request)**

Once you have a key, provide it via either:

- `Regira:LicenseKey` in `appsettings.json`, or
- the `Regira__LicenseKey` environment variable.

For local development only, set `REGIRA_LICENSE_SKIP_VALIDATION=true` to bypass validation.
