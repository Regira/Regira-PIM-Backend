using Microsoft.Extensions.DependencyInjection;
using PIM.Core.Abstractions;
using PIM.Core.Constants;
using PIM.Data;
using PIM.Models.Orders;
using PIM.Services.Entities.Orders;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Extensions;
using Regira.Entities.Services.Abstractions;

namespace PIM.DependencyInjection.Orders;

public static class OrderServiceConfiguration
{
    public static IEntityServiceCollection<PimDbContext> AddOrders(this IEntityServiceCollection<PimDbContext> services)
    {
        services.For<Order, OrderSearchObject, OrderSortBy, OrderIncludes>(e =>
        {
            e.AddFilter<OrderQueryBuilder>();
            e.AddSortBy<OrderSortingQueryBuilder>();
            e.AddIncludes<OrderIncludingQueryBuilder>();
            e.Related(x => x.OrderLines, item => item.OrderLines?.SetSortOrder());
            e.AddPrepper<OrderPrepper>();
            e.AddNormalizer<OrderNormalizer>();
            e.AddTransient<IOrderServiceValidator, OrderManagerValidator>();
            e.UseEntityService<IEntityService<Order, OrderSearchObject, OrderSortBy, OrderIncludes>>(p =>
            {
                return p.GetRequiredService<IAppContext>().AppType switch
                {
                    PimAppTypes.Public => new OrderManagerPersonal(p.GetRequiredService<IOrderServiceValidator>(), p.GetRequiredService<IOrderContext>()),
                    //PimAppTypes.Manager => new OrderManagerValidator(p.GetRequiredService<IEntityRepository<Order, OrderSearchObject, OrderSortBy, OrderIncludes>>()),
                    _ => new OrderManagerValidator(p.GetRequiredService<IEntityRepository<Order, OrderSearchObject, OrderSortBy, OrderIncludes>>())
                };
            });
        });

        return services;
    }
}
