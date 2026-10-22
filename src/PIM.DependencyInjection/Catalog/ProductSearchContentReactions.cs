using Microsoft.Extensions.DependencyInjection;
using PIM.Services.Entities.Catalog.Products;
using Regira.System.Hosting.Background.Abstractions;

namespace PIM.DependencyInjection.Catalog;

/// <summary>
/// What the reactors on facets, products and parties do once a rename is committed: queue a
/// <see cref="ProductSearchContentRefresher"/> run in the background. A widely used facet is embedded in many products,
/// and a reactor runs inside the call that committed the rename.
/// </summary>
internal static class ProductSearchContentReactions
{
    public static Task Refresh(IServiceProvider services, Func<ProductSearchContentRefresher, CancellationToken, Task<int>> refresh)
    {
        services.GetRequiredService<IBackgroundQueueManager>()
            .Execute((sp, task) => refresh(sp.GetRequiredService<ProductSearchContentRefresher>(), task.Token));
        return Task.CompletedTask;
    }
}
