using Microsoft.EntityFrameworkCore;
using PIM.Core.Constants;
using PIM.Data;
using PIM.Models.Catalog.Products;
using PIM.Services.Entities.Validation;
using Regira.Entities.Validators.Abstractions;

namespace PIM.Services.Entities.Catalog.Products;

/// <summary>
/// A product's components: each needs a quantity, and none may contain the product itself, directly or further down
/// its bill of materials — a cycle there makes every tree of it endless.
/// </summary>
public class ProductValidator(PimDbContext dbContext) : EntityValidatorBase<Product>
{
    public override async Task Validate(IEntityValidatorContext<Product> context, CancellationToken token = default)
    {
        var item = context.Item;
        // null = the save leaves the components as they are
        if (context.Operation == EntityWriteOperation.Remove || item.Components == null)
        {
            return;
        }

        var components = item.Components.Where(c => dbContext.Entry(c).State != EntityState.Deleted).ToList();
        if (components.Any(c => c.Quantity <= 0))
        {
            context.AddError(nameof(Product.Components), ValidationMessages.ComponentQuantity);
        }

        // nothing references a product that is not stored yet, so a new one closes no cycle
        if (item.Id == 0)
        {
            return;
        }

        // the components as stored: this save adds none that could close a cycle
        var storedComponents = await dbContext.Set<ProductComponent>().Where(pc => pc.AssemblyId == item.Id).Select(pc => pc.ComponentId).ToListAsync(token);
        if (components.Select(c => c.ComponentId).ToHashSet().SetEquals(storedComponents))
        {
            return;
        }

        // the product's stored assemblies are not part of this save: reaching the product itself closes the cycle
        var cycle = await HierarchyCycleFinder.Find(item.Id, components.Select(c => c.ComponentId), [item.Id], ComponentsOf, token);
        if (cycle != null)
        {
            var title = await dbContext.Products.IgnoreQueryFilters()
                .Where(p => p.Id == cycle.Value.Child)
                .Select(p => p.Title)
                .FirstOrDefaultAsync(token);
            context.AddError(nameof(Product.Components), ValidationMessages.ComponentCycle, new { component = title });
        }
    }

    async Task<IEnumerable<(int Parent, int Child)>> ComponentsOf(IReadOnlyCollection<int> productIds, CancellationToken token)
    {
        var edges = await dbContext.Set<ProductComponent>()
            .Where(pc => productIds.Contains(pc.AssemblyId))
            .Select(pc => new { pc.AssemblyId, pc.ComponentId })
            .ToListAsync(token);
        return edges.Select(e => (e.AssemblyId, e.ComponentId));
    }
}
