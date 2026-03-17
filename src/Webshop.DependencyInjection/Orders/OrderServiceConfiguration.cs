using Regira.Entities.DependencyInjection.Preppers;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Extensions;
using Webshop.Data;
using Webshop.Models.Orders;
using Webshop.Services.Entities.Orders;

namespace Webshop.DependencyInjection.Orders;

public static class OrderServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddOrders(this IEntityServiceCollection<WebshopDbContext> services, bool isAdmin)
    {
        services.For<Order, OrderSearchObject, OrderSortBy, OrderIncludes>(e =>
        {
            e.AddQueryFilter<OrderQueryBuilder>();
            e.SortBy<OrderSortingQueryBuilder>();
            e.Includes<OrderIncludingQueryBuilder>();
            e.Related(x => x.OrderLines, item => item.OrderLines?.SetSortOrder());
            e.AddPrepper<OrderPrepper>();
            e.AddTransient<IOrderService, OrderManager>();
            e.AddNormalizer<OrderNormalizer>();
            if (isAdmin)
            {
                e.UseEntityService<OrderManager>();
            }
            else
            {
                e.UseEntityService<PersonalOrderManager>();
            }
        });
        
        return services;
    }
}
