using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.Preppers;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Extensions;
using Regira.Entities.Services.Abstractions;
using Webshop.Core.Abstractions;
using Webshop.Core.Constants;
using Webshop.Data;
using Webshop.Models.Orders;
using Webshop.Services.Entities.Orders;

namespace Webshop.DependencyInjection.Orders;

public static class OrderServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddOrders(this IEntityServiceCollection<WebshopDbContext> services)
    {
        services.For<Order, OrderSearchObject, OrderSortBy, OrderIncludes>(e =>
        {
            e.AddQueryFilter<OrderQueryBuilder>();
            e.SortBy<OrderSortingQueryBuilder>();
            e.Includes<OrderIncludingQueryBuilder>();
            e.Related(x => x.OrderLines, item => item.OrderLines?.SetSortOrder());
            e.AddPrepper<OrderPrepper>();
            e.AddNormalizer<OrderNormalizer>();
            e.AddTransient<IOrderServiceValidator, OrderManagerValidator>();
            e.UseEntityService<IEntityService<Order, OrderSearchObject, OrderSortBy, OrderIncludes>>(p =>
            {
                return p.GetRequiredService<IAppContext>().AppType switch
                {
                    WebshopAppTypes.Public => new OrderManagerPersonal(p.GetRequiredService<IOrderServiceValidator>(), p.GetRequiredService<IOrderContext>()),
                    //WebshopAppTypes.Manager => new OrderManagerValidator(p.GetRequiredService<IEntityRepository<Order, OrderSearchObject, OrderSortBy, OrderIncludes>>()),
                    _ => new OrderManagerValidator(p.GetRequiredService<IEntityRepository<Order, OrderSearchObject, OrderSortBy, OrderIncludes>>())
                };
            });
        });

        return services;
    }
}
