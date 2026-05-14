# Webshop API — Regira Entities Example

> **⚠️ Reference material — mixed fidelity examples.**
> - Some snippets in this file are abbreviated, while others are close to production-ready and only omit surrounding project context, imports, or neighboring files.
> - Before using any configuration call (especially lambdas inside `UseEntities`), verify the exact signature in [`entities.signatures.md`](./entities.signatures.md).
> - Namespace imports are listed in [`entities.namespaces.md`](./entities.namespaces.md).

*Treat these as working patterns, not drop-in files.*

For the correct **namespaces**: see [`entities.namespaces.md`](./entities.namespaces.md).

## Structure

With FastEndpoints auto-registration, no `Controllers/` folder is needed. Add one only when using `EntityControllerBase` or manual endpoint classes.

```
Webshop.API/
├── Data/
│   └── WebshopDbContext.cs
├── Entities/
│   ├── Categories/
│   │   ├── Category.cs
│   │   ├── CategoryDto.cs
│   │   ├── CategoryIncludes.cs
│   │   ├── CategoryInputDto.cs
│   │   ├── CategoryProcessor.cs
│   │   ├── CategorySearchObject.cs
│   │   ├── CategoryServiceConfiguration.cs
│   │   ├── RelatedCategory.cs
│   │   ├── RelatedCategoryDto.cs
│   │   └── RelatedCategoryInputDto.cs
│   ├── Customers/
│   │   ├── Customer.cs
│   │   ├── CustomerDto.cs
│   │   ├── CustomerInputDto.cs
│   │   └── CustomerServiceConfiguration.cs
│   ├── Orders/
│   │   ├── Order.cs
│   │   ├── OrderDto.cs
│   │   ├── OrderIncludes.cs
│   │   ├── OrderInputDto.cs
│   │   ├── OrderLine.cs
│   │   ├── OrderLineDto.cs
│   │   ├── OrderLineInputDto.cs
│   │   ├── OrderManager.cs
│   │   ├── OrderNormalizer.cs
│   │   ├── OrderQueryBuilder.cs
│   │   ├── OrderSearchObject.cs
│   │   ├── OrderServiceConfiguration.cs
│   │   └── OrderStatus.cs
│   └── Products/
│       ├── Product.cs
│       ├── ProductCategory.cs
│       ├── ProductCategoryDto.cs
│       ├── ProductCategoryInputDto.cs
│       ├── ProductDto.cs
│       ├── ProductInputDto.cs
│       ├── ProductQueryBuilder.cs
│       ├── ProductSearchObject.cs
│       ├── ProductServiceConfiguration.cs
│       └── ProductSortBy.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── Program.cs
```

## Setup

see [`entities.setup.md`](./entities.setup.md)

## DbContext

```csharp
// Data/WebshopDbContext.cs
public class WebshopDbContext(DbContextOptions<WebshopDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<RelatedCategory> RelatedCategories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductCategory> ProductCategories { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderLine> OrderLines { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(18, 2); // Regira.DAL.EFcore.Extensions

        modelBuilder.Entity<RelatedCategory>(e =>
        {
            e.HasOne(c => c.Parent).WithMany(e => e.ChildEntities).HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Child).WithMany(e => e.ParentEntities).HasForeignKey(c => c.ChildId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ProductCategory>(e =>
        {
            e.HasKey(pc => pc.Id);
            e.HasIndex(pc => new { pc.ProductId, pc.CategoryId }).IsUnique();
            e.HasOne(pc => pc.Product).WithMany(p => p.Categories).HasForeignKey(pc => pc.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pc => pc.Category).WithMany().HasForeignKey(pc => pc.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasIndex(c => c.Email).IsUnique();
            e.HasMany(c => c.Orders).WithOne(o => o.Customer).HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.Code).IsUnique();
            e.HasMany(o => o.OrderLines).WithOne(ol => ol.Order).HasForeignKey(ol => ol.OrderId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<OrderLine>(e =>
            e.HasOne(ol => ol.Product).WithMany().HasForeignKey(ol => ol.ProductId).OnDelete(DeleteBehavior.Restrict));
    }
}
```

## Category entity

```csharp
// Entities/Categories/Category.cs
public class Category : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasNormalizedContent, IArchivable
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public bool IsArchived { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<RelatedCategory>? ParentEntities { get; set; }
    public ICollection<RelatedCategory>? ChildEntities { get; set; }
    [NotMapped] public int? ProductCount { get; set; }  // filled by CategoryProcessor
}

// Entities/Categories/RelatedCategory.cs — self-referential join entity
public class RelatedCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }
    public Category Child { get; set; } = null!;
    public Category Parent { get; set; } = null!;
}

// Entities/Categories/CategorySearchObject.cs
public class CategorySearchObject : SearchObject
{
    public ICollection<int>? ParentId { get; set; }
    public ICollection<int>? ChildId { get; set; }
    public bool? IsRoot { get; set; }
}

// Entities/Categories/CategoryIncludes.cs
[Flags] public enum CategoryIncludes { Default=0, Parents=1<<0, Children=1<<1, All=Parents|Children }

// Entities/Categories/CategoryDto.cs
public class CategoryCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public int? ArticleCount { get; set; }
}
public class CategoryDto : CategoryCoreDto
{
    public ICollection<ParentCategoryDto>? ParentEntities { get; set; }
    public ICollection<ChildCategoryDto>? ChildEntities { get; set; }
    public int? ProductCount { get; set; }
}

// Entities/Categories/RelatedCategoryDto.cs
public class RelatedCategoryDto {
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }
}
public class ParentCategoryDto : RelatedCategoryDto { public CategoryCoreDto Parent { get; set; } = null!; }
public class ChildCategoryDto  : RelatedCategoryDto { public CategoryCoreDto Child { get; set; } = null!; }

// Entities/Categories/CategoryInputDto.cs + RelatedCategoryInputDto.cs
public class CategoryInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    public ICollection<RelatedCategoryInputDto>? ParentEntities { get; set; }
    public ICollection<RelatedCategoryInputDto>? ChildEntities { get; set; }
}
public class RelatedCategoryInputDto { public int Id,ChildId,ParentId; }

// Entities/Categories/CategoryProcessor.cs — separate class processor using DbContext DI
public class CategoryProcessor(WebshopDbContext dbContext) : IEntityProcessor<Category, CategoryIncludes>
{
    public async Task Process(IList<Category> items, CategoryIncludes? includes, CancellationToken token = default)
    {
        var itemIds = items.Select(x => x.Id).ToList();
        var counts = await dbContext.Categories
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, Count = dbContext.ProductCategories.Count(pc => pc.CategoryId == x.Id) })
            .ToDictionaryAsync(x => x.Id, v => v.Count);
        foreach (var item in items)
            item.ProductCount = counts.TryGetValue(item.Id, out var count) ? count : null;
    }
}

// Entities/Categories/CategoryServiceConfiguration.cs
public static IEntityServiceCollection<WebshopDbContext> AddCategories(this IEntityServiceCollection<WebshopDbContext> services)
{
    services.For<Category, CategorySearchObject, EntitySortBy, CategoryIncludes>(e =>
    {
        e.Filter((query, so) =>
        {
            if (so?.ParentId?.Any() == true)
              query = query.Where(x => x.ParentEntities!.Any(pe => so.ParentId.Contains(pe.ParentId)));
            if (so?.ChildId?.Any() == true)
              query = query.Where(x => x.ChildEntities!.Any(ce => so.ChildId.Contains(ce.ChildId)));
            if (so?.IsRoot != null)
                query = so.IsRoot.Value ? query.Where(x => !x.ParentEntities!.Any()) : query.Where(x => x.ParentEntities!.Any());
            return query;
        });
        e.SortBy((query, sortBy) => query.OrderByDescending(x => x.Title));
        e.Includes((query, includes) =>
        {
            if (includes?.HasFlag(CategoryIncludes.Parents) == true)
                query = query.Include(x => x.ParentEntities!).ThenInclude(x => x.Parent);
            if (includes?.HasFlag(CategoryIncludes.Children) == true)
                query = query.Include(x => x.ChildEntities!).ThenInclude(x => x.Child);
            return query;
        });
        e.AddProcessor<CategoryProcessor>();
        e.Related(x => x.ParentEntities);
        e.Related(x => x.ChildEntities);
    });
    return services;
}
```

## Product entity

```csharp
// Entities/Products/Product.cs
public class Product : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    public decimal Price { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<ProductCategory>? Categories { get; set; }
}

// Entities/Products/ProductCategory.cs — many-to-many join
public class ProductCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}

// Entities/Products/ProductSearchObject.cs
public class ProductSearchObject : SearchObject
{
    public ICollection<int>? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}

// Entities/Products/ProductSortBy.cs
public enum ProductSortBy { Default=0, Title, TitleDesc, Price, PriceDesc }

// Entities/Products/ProductDto.cs
public class ProductDto { public int Id; public string Title=null!; public string? Description; public decimal Price; public DateTime Created; public DateTime? LastModified; public ICollection<ProductCategoryDto>? Categories; }
public class ProductCategoryDto { public int Id,ProductId,CategoryId; public CategoryDto? Category; }

// Entities/Products/ProductInputDto.cs
public class ProductInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    public decimal Price { get; set; }
    public ICollection<ProductCategoryInputDto>? Categories { get; set; }
}
public class ProductCategoryInputDto { public int Id,ProductId,CategoryId; }

// Entities/Products/ProductQueryBuilder.cs — separate class, handles all product filtering
public class ProductQueryBuilder(WebshopDbContext dbContext)
    : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (so == null) return query;
        if (so.CategoryId?.Any() == true) query = query.Where(x => x.Categories!.Any(pc => so.CategoryId.Contains(pc.CategoryId)));
        if (so.MinPrice.HasValue) query = query.Where(p => p.Price >= so.MinPrice.Value);
        if (so.MaxPrice.HasValue) query = query.Where(p => p.Price <= so.MaxPrice.Value);
        return query;
    }
}

// Entities/Products/ProductServiceConfiguration.cs
public static IEntityServiceCollection<WebshopDbContext> AddProducts(this IEntityServiceCollection<WebshopDbContext> services)
{
    services.For<Product, ProductSearchObject, ProductSortBy, EntityIncludes>(e =>
    {
        e.AddFilter<ProductQueryBuilder>();
        e.SortBy((query, sortBy) =>
        {
            if (typeof(IOrderedQueryable).IsAssignableFrom(query.Expression.Type) && query is IOrderedQueryable<Product> sorted)
                return sortBy switch
                {
                    ProductSortBy.Title => sorted.ThenBy(x => x.Title),
                    ProductSortBy.TitleDesc => sorted.ThenByDescending(x => x.Title),
                    ProductSortBy.Price => sorted.ThenBy(x => x.Price),
                    ProductSortBy.PriceDesc => sorted.ThenByDescending(x => x.Price),
                    _ => sorted.ThenByDescending(x => x.Title)
                };
            return sortBy switch
            {
                ProductSortBy.Title => query.OrderBy(x => x.Title),
                ProductSortBy.TitleDesc => query.OrderByDescending(x => x.Title),
                ProductSortBy.Price => query.OrderBy(x => x.Price),
                ProductSortBy.PriceDesc => query.OrderByDescending(x => x.Price),
                _ => query.OrderByDescending(x => x.Title)
            };
        });
        // Related() only handles child collection synchronization.
        e.Related(x => x.Categories);
        e.Includes((query, includes) => {
            if (includes?.HasFlag(EntityIncludes.All) == true)
                query = query.Include(x => x.Categories!).ThenInclude(pc => pc.Category);
            return query;
        });
    });
    return services;
}
```

## Customer entity

> **Note:** This entity uses `Guid` as the primary key to demonstrate the non-int key workflow. 
In real projects, choose the key type based on your requirements — `int` (auto-increment) is the default and most common choice.

```csharp
// Entities/Customers/Customer.cs — uses IEntity<Guid> (non-int key)
public class Customer : IEntity<Guid>, IHasTimestamps, IHasNormalizedContent
{
    public Guid Id { get; set; }
    [Required, MaxLength(256)] public string Name { get; set; } = null!;
    [Required, MaxLength(256)] public string Email { get; set; } = null!;
    [MaxLength(512), Normalized(SourceProperties = [nameof(Name), nameof(Email)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<Order>? Orders { get; set; }
}

// Entities/Customers/CustomerDto.cs + CustomerInputDto.cs
public class CustomerDto { public Guid Id; public string Name=null!,Email=null!; public DateTime Created; public DateTime? LastModified; }
public class CustomerInputDto
{
    public Guid? Id { get; set; }  // nullable — omit on create, set on update
    [Required, MaxLength(256)] public string Name { get; set; } = null!;
    [Required, MaxLength(256), EmailAddress] public string Email { get; set; } = null!;
}

// Entities/Customers/CustomerServiceConfiguration.cs — For<TEntity, TKey> overload for non-int key
public static IEntityServiceCollection<WebshopDbContext> AddCustomers(this IEntityServiceCollection<WebshopDbContext> services)
{
    services.For<Customer, Guid>(e =>
    {
        e.SortBy(query => query.OrderBy(x => x.Name));
        e.Prepare(item => item.Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id); // inline prepper
    });
    return services;
}
```

## Order + OrderLine entities

```csharp
// Entities/Orders/OrderStatus.cs
public enum OrderStatus { Pending=0, Processing, Shipped, Delivered, Cancelled }

// Entities/Orders/Order.cs — IHasAggregateKey for event-sourcing/idempotency; IHasNormalizedContent for search
public class Order : IEntityWithSerial, IHasAggregateKey, IHasTimestamps, IHasCode, IHasNormalizedContent
{
    public int Id { get; set; }
    public Guid? AggregateKey { get; set; }
    [MaxLength(16)] public string? Code { get; set; }
    public Guid CustomerId { get; set; }  // FK matches Customer's Guid key
    public Customer? Customer { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Total { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<OrderLine>? OrderLines { get; set; }
    [MaxLength(1024)] public string? NormalizedContent { get; set; }
}

// Entities/Orders/OrderLine.cs
public class OrderLine : IEntityWithSerial, IHasTimestamps, ISortable
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public int SortOrder { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

// Entities/Orders/OrderSearchObject.cs
public class OrderSearchObject : SearchObject
{
    public string? Code { get; set; }
    public ICollection<Guid>? CustomerId { get; set; }  // Guid FK
    public ICollection<int>? ProductId { get; set; }
    public ICollection<int>? CategoryId { get; set; }
    public ICollection<OrderStatus>? Status { get; set; }
}

// Entities/Orders/OrderIncludes.cs
[Flags] public enum OrderIncludes { Default=0, Customer=1<<0, OrderLines=1<<1, All=Customer|OrderLines }

// Entities/Orders/OrderDto.cs + OrderLineDto.cs
public class OrderDto { public int Id; public Guid? AggregateKey; public string? Code; public Guid CustomerId; public CustomerDto? Customer; public OrderStatus Status; public decimal Total; public DateTime Created; public DateTime? LastModified; public ICollection<OrderLineDto>? OrderLines; }
public class OrderLineDto { public int Id,OrderId,ProductId,Quantity,SortOrder; public ProductDto? Product; public decimal UnitPrice,SubTotal; }

// Entities/Orders/OrderInputDto.cs + OrderLineInputDto.cs
public class OrderInputDto { public int Id; public Guid? AggregateKey; [MaxLength(16)] public string? Code; public Guid CustomerId; public OrderStatus Status=OrderStatus.Pending; public ICollection<OrderLineInputDto>? OrderLines; }
public class OrderLineInputDto { public int Id,OrderId,ProductId,Quantity; public decimal UnitPrice; }

// Entities/Orders/OrderNormalizer.cs — enriches NormalizedContent with customer + product text
public class OrderNormalizer(WebshopDbContext dbContext) : EntityNormalizerBase<Order>
{
    public override async Task HandleNormalize(Order item, CancellationToken token = default)
    {
        await base.HandleNormalize(item, token);
        var productIds = item.OrderLines!.Select(ol => ol.ProductId).ToList();
        var productContents = await dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.NormalizedContent);
        var linesContent = item.OrderLines != null
            ? string.Join(" ", item.OrderLines.Select(ol => productContents.TryGetValue(ol.ProductId, out var c) ? c : null))
            : string.Empty;
        item.NormalizedContent = $"{item.NormalizedContent} {item.Customer?.NormalizedContent} {linesContent}".Trim();
    }
}

// Entities/Orders/OrderQueryBuilder.cs — implements IFilteredQueryBuilder directly; uses QueryExtensions
public class OrderQueryBuilder : IFilteredQueryBuilder<Order, int, OrderSearchObject>
{
    public IQueryable<Order> Build(IQueryable<Order> query, OrderSearchObject? so)
    {
        if (so == null) return query;
        // FilterCode requires IHasCode — Order implements it; inline alternative: query.Where(x => x.Code == so.Code)
        if (!string.IsNullOrWhiteSpace(so.Code)) query = query.FilterCode(so.Code);
        if (so.CustomerId?.Any() == true) query = query.Where(x => so.CustomerId.Contains(x.CustomerId));
        if (so.ProductId?.Any() == true) query = query.Where(x => x.OrderLines!.Any(ol => so.ProductId.Contains(ol.ProductId)));
        if (so.CategoryId?.Any() == true) query = query.Where(x => x.OrderLines!.Any(ol => ol.Product!.Categories!.Any(pc => so.CategoryId.Contains(pc.CategoryId))));
        if (so.Status != null) query = query.Where(x => so.Status.Contains(x.Status));
        return query;
    }
}

// Entities/Orders/OrderManager.cs — EntityWrappingServiceBase with validation + EntityInputException
public interface IOrderService : IEntityService<Order, OrderSearchObject, EntitySortBy, OrderIncludes>;
public class OrderManager(IEntityRepository<Order, OrderSearchObject, EntitySortBy, OrderIncludes> service)
    : EntityWrappingServiceBase<Order, OrderSearchObject, EntitySortBy, OrderIncludes>(service), IOrderService
{
    public override Task Add(Order item, CancellationToken token = default) { Validate(item); if (string.IsNullOrWhiteSpace(item.Code)) item.Code = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}"; return base.Add(item, token); }
    public override Task<Order?> Modify(Order item, CancellationToken token = default) { Validate(item); return base.Modify(item, token); }
    public override Task Save(Order item, CancellationToken token = default) { Validate(item); return base.Save(item, token); }
    private static void Validate(Order item)
    {
        if (item.OrderLines?.Any() != true)
            throw new EntityInputException<Order>("Saving order failed")
            {
                InputErrors = { ["OrderLines"] = "Order must contain at least one order line." }
            };
    }
}

// Entities/Orders/OrderServiceConfiguration.cs
public static IEntityServiceCollection<WebshopDbContext> AddOrders(this IEntityServiceCollection<WebshopDbContext> services)
{
    services.For<Order, OrderSearchObject, EntitySortBy, OrderIncludes>(e =>
    {
        e.AddFilter<OrderQueryBuilder>();
        e.SortBy((query, sortBy) => query.OrderByDescending(x => x.Created));
        e.Includes((query, includes) => query
            .Include(x => x.Customer!)
            .Include(x => x.OrderLines!.OrderBy(l => l.SortOrder))
                .ThenInclude(ol => ol.Product!));
        e.Related(x => x.OrderLines, item => item.OrderLines?.SetSortOrder()); // OrderLine must implement ISortable; if using ICollection<T> where T does not, cast: (item.OrderLines as IEnumerable<ISortable>)?.SetSortOrder()
        e.Prepare((order, original) =>
        {
            if (order.OrderLines?.Any() == true)
                order.Total = order.OrderLines.Sum(ol => ol.Quantity * ol.UnitPrice);
            return Task.CompletedTask;
        });
        e.AddNormalizer<OrderNormalizer>();
        e.AddTransient<IOrderService, OrderManager>();  // enables typed IOrderService injection
        e.UseEntityService<OrderManager>();             // replaces default EntityRepository
    });
    return services;
}
```

## Web Endpoints

### FastEndpoints (Preferred)

All entity CRUD routes are auto-registered by a single call in `Program.cs`. No per-entity code needed.

```csharp
// Program.cs
builder.Services.AddFastEndpoints();
// ...
app.UseFastEndpoints();
app.MapEntityEndpoints();
// Auto-registered routes (Humanizer handles pluralization):
//   /api/products/*, /api/categories/*, /api/customers/*, /api/orders/*

// Routes per entity (example for Product):
// GET    /api/products/{id}    — Details
// GET    /api/products         — List
// POST   /api/products         — Create
// POST   /api/products/save    — Save (upsert)
// PUT    /api/products/{id}    — Modify
// DELETE /api/products/{id}    — Delete

// Use For<TEntity>(route) only for fully custom paths:
// app.MapEntityEndpoints(configure: o => o.For<Product>("v2/shop/items"));
```

### Controllers (Advanced)

Use `EntityControllerBase` when the project already relies on MVC controllers:

```csharp
// Controllers/CategoryController.cs ~ For<Category, CategorySearchObject, EntitySortBy, CategoryIncludes>()
[ApiController, Route("categories")]
public class CategoryController : EntityControllerBase<Category, CategorySearchObject, EntitySortBy, CategoryIncludes, CategoryDto, CategoryInputDto>;

// Controllers/ProductController.cs ~ For<Product, ProductSearchObject, ProductSortBy, EntityIncludes>()
[ApiController, Route("products")]
public class ProductController : EntityControllerBase<Product, ProductSearchObject, ProductSortBy, EntityIncludes, ProductDto, ProductInputDto>;

// Controllers/CustomerController.cs ~ For<Customer, Guid, SearchObject<Guid>>()
// Guid key: uses TKey overload with SearchObject<Guid>
[ApiController, Route("customers")]
public class CustomerController : EntityControllerBase<Customer, Guid, SearchObject<Guid>, CustomerDto, CustomerInputDto>;

// Controllers/OrderController.cs ~ For<Order, OrderSearchObject, EntitySortBy, OrderIncludes>()
[ApiController, Route("orders")]
public class OrderController : EntityControllerBase<Order, OrderSearchObject, EntitySortBy, OrderIncludes, OrderDto, OrderInputDto>;
```

---

## Additional Patterns

### Inline processor

```csharp
e.Process((items, includes) =>
{
    foreach (var item in items)
        item.DisplayPrice = $"€{item.Price:F2}";
    return Task.CompletedTask;
});
```

### Prepper — with DbContext / separate class

```csharp
// With DbContext:
e.Prepare(async (item, dbContext) =>
{
    item.TotalAmount = item.OrderItems?.Sum(x => x.Quantity * x.UnitPrice) ?? 0;
    await Task.CompletedTask;
});

// Separate class:
public class ProductPrepper : EntityPrepperBase<Product>
{
    public override Task Prepare(Product modified, Product? original, CancellationToken token = default)
    {
        modified.Slug ??= modified.Title.ToLowerInvariant().Replace(' ', '-');
        return Task.CompletedTask;
    }
}
// Registration: e.AddPrepper<ProductPrepper>();
```

### Primers

```csharp
// Entity-specific primer:
public class ProductPrimer : EntityPrimerBase<Product>
{
    public override Task PrepareAsync(Product entity, EntityEntry entry, CancellationToken token = default)
    {
        if (entry.State == EntityState.Added)
            entity.Code ??= Guid.NewGuid().ToString("N")[..8].ToUpper();
        return Task.CompletedTask;
    }
}
// Registration: e.AddPrimer<ProductPrimer>();

// Global primer: options.AddPrimer<YourGlobalPrimer>();
```

### AddMapping - nested DTOs and Related collections

Use `AddMapping<TSource, TTarget>()` when nested output DTOs or child `InputDto` collections are not obvious enough for convention-based mapping alone.

```csharp
services.For<Product, ProductSearchObject, ProductSortBy, EntityIncludes>(e =>
{
    e.UseMapping<ProductDto, ProductInputDto>();

    // Same-type registration looks unusual, but it tells the mapper how to project nested DTO collections.
    e.AddMapping<ProductCategoryDto, ProductCategoryDto>();

    // Cross-type registration is needed when InputDto child collections are synchronized via Related().
    e.AddMapping<ProductCategoryInputDto, ProductCategory>();

    e.Related(x => x.Categories);
});
```

### AfterMapper

```csharp
// Inline:
e.UseMapping<ProductDto, ProductInputDto>()
    .After((entity, dto) =>
    {
        dto.DisplayName = $"{entity.Title} - €{entity.Price:F2}";
    })
    .AfterInput((inputDto, entity) =>
    {
        // runs after InputDto → Entity mapping
    });

// Separate class (with DI):
public class ProductAfterMapper(IHttpContextAccessor httpContextAccessor) : EntityAfterMapperBase<Product, ProductDto>
{
    public override void AfterMap(Product source, ProductDto target)
        => target.ImageUrl = $"https://{httpContextAccessor.HttpContext?.Request.Host}/images/{source.Id}";
}
// Registration: e.UseMapping<ProductDto, ProductInputDto>().After<ProductAfterMapper>();
// Global:       options.AddAfterMapper<MyGlobalAfterMapper>();
```

### Database initialization and seeding

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WebshopDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    if (!await dbContext.Orders.AnyAsync())
    {
        dbContext.Orders.AddRange(SeedOrders());
        await dbContext.SaveChangesAsync();
    }
}
```

- For the default SQLite starter/test setup, pair seeding with `EnsureCreated()`.
- Use `DbContext` directly when you intentionally want raw EF Core startup seeding.
- If you seed through `IEntityService`, remember that preppers and primers still run and `SaveChanges()` remains explicit.

### IQKeywordHelper — Q full-text search

```csharp
public class ProductQueryBuilder(IQKeywordHelper qHelper) : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (!string.IsNullOrWhiteSpace(so?.Q))
        {
            var keywords = qHelper.Parse(so.Q);
            foreach (var keyword in keywords)
                query = query.Where(x => EF.Functions.Like(x.NormalizedContent, keyword.QW));
        }
        return query;
    }
}
```

### Global filter query builder

```csharp
// Separate class — applies to all entities implementing the interface:
public class FilterByTenantQueryBuilder(ITenantContext tenantContext) : GlobalFilteredQueryBuilderBase<ITenantEntity, int>
{
    public override IQueryable<ITenantEntity> Build(IQueryable<ITenantEntity> query, ISearchObject<int>? so)
        => query.Where(x => x.TenantId == tenantContext.CurrentTenantId);
}
// Registration: options.AddGlobalFilterQueryBuilder<FilterByTenantQueryBuilder>();

// Built-in Q search for all IHasNormalizedContent entities:
// ⚠️ UseDefaults() already calls this automatically — only register manually if you are
//    NOT using UseDefaults() and want to enable Q-based full-text search globally.
options.AddGlobalFilterQueryBuilder<FilterHasNormalizedContentQueryBuilder>();
```

### Global normalizer

```csharp
// Uses INormalizer to manually control normalization output:
public class ProductNormalizer(INormalizer normalizer) : EntityNormalizerBase<Product>
{
    public override async Task HandleNormalize(Product item, CancellationToken token = default)
        => item.NormalizedContent = await normalizer.Normalize($"{item.Title} {item.Description}".Trim());
}
// Per-entity: e.AddNormalizer<ProductNormalizer>();
// Global:     options.AddNormalizer<IHasPhone, PhoneNormalizer>();
```

### Attachments

```csharp
// Attachment entity:
public class ProductAttachment : EntityAttachment<int, int>
{
    public override string ObjectType => nameof(Product);
}

// Entity:
public class Product : IEntityWithSerial, IHasAttachments, IHasAttachments<ProductAttachment>
{
    public bool? HasAttachment { get; set; }
    public ICollection<ProductAttachment>? Attachments { get; set; }
    ICollection<IEntityAttachment>? IHasAttachments.Attachments
    {
        get => Attachments?.Cast<IEntityAttachment>().ToArray();
        set => Attachments = value?.Cast<ProductAttachment>().ToArray();
    }
}

// Controller:
[ApiController, Route("api/products/{objectId}/attachments")]
public class ProductAttachmentsController : EntityAttachmentControllerBase<ProductAttachment, int, int> { }

// DbContext:
public DbSet<Attachment> Attachments { get; set; } = null!;
public DbSet<ProductAttachment> ProductAttachments { get; set; } = null!;
// OnModelCreating:
modelBuilder.Entity<ProductAttachment>()
    .HasOne(x => x.Attachment).WithMany().HasForeignKey(x => x.AttachmentId);
modelBuilder.Entity<Product>(entity =>
    entity.HasMany(e => e.Attachments).WithOne().HasForeignKey(e => e.ObjectId).HasPrincipalKey(e => e.Id));

// DI:
services.UseEntities<AppDbContext>(/* ... */)
    .WithAttachments(sp => new BinaryFileService(
        new FileSystemOptions { RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads") }));
```

### Query extensions reference

> Each `QueryExtensions` method requires the entity to implement the listed interface. If it does not,
> use inline LINQ (e.g. `query.Where(x => x.Code == so.Code)`) as a drop-in replacement.
> See [`entities.signatures.md`](./entities.signatures.md) — §Query Extensions for full interface constraints.

```csharp
// From Regira.Entities.EFcore.QueryBuilders (QueryExtensions):
query.FilterId(so.Id)                   // requires IEntity<TKey>
query.FilterIds(so.Ids)                 // requires IEntity<TKey>
query.FilterExclude(so.Exclude)         // requires IEntity<TKey>
query.FilterCode(so.Code)               // requires IHasCode
query.FilterTitle(keywords)             // requires IHasTitle
query.FilterNormalizedTitle(keywords)   // requires IHasNormalizedTitle
query.FilterCreated(so.MinCreated, so.MaxCreated)               // requires IHasCreated
query.FilterLastModified(so.MinLastModified, so.MaxLastModified) // requires IHasLastModified
query.FilterTimestamps(minCreated, maxCreated, minModified, maxModified) // requires IHasTimestamps
query.FilterQ(keywords)                 // requires IHasNormalizedContent
query.FilterArchivable(so.IsArchived)   // requires IArchivable
query.FilterHasAttachment(so.HasAttachment) // requires IHasAttachments
query.SortQuery<TEntity, TKey>()
query.PageQuery(pagingInfo)
query.PageQuery(pageSize: 20, page: 1)
```
