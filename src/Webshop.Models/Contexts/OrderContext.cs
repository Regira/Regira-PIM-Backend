using Microsoft.AspNetCore.Http;

namespace Webshop.Models.Contexts;

public interface IOrderContext
{
    int? CustomerId { get; }
    int? OrganizationId { get; }
    DateTime? OrderDate { get; }
}
public class OrderContext(IHttpContextAccessor httpContextAccessor) : IOrderContext
{
    public int? CustomerId => int.TryParse(httpContextAccessor.HttpContext?.Request.Query["customerId"], out var customerId) ? customerId : null;
    public int? OrganizationId => int.TryParse(httpContextAccessor.HttpContext?.Request.Query["organizationId"], out var organizationId) ? organizationId : null;
    public DateTime? OrderDate => DateTime.TryParse(httpContextAccessor.HttpContext?.Request.Query["orderDate"], out var orderDate) ? orderDate : null;
}
