using Microsoft.AspNetCore.Http;
using Webshop.Models.Contexts;

namespace Webshop.Web;

public class OrderContext(IHttpContextAccessor httpContextAccessor) : IOrderContext
{
    public int? CustomerId => int.TryParse(httpContextAccessor.HttpContext?.Request.Query["customerId"], out var customerId) ? customerId : null;
    public int? OrganizationId => int.TryParse(httpContextAccessor.HttpContext?.Request.Query["organizationId"], out var organizationId) ? organizationId : null;
    public DateTime? OrderDate => DateTime.TryParse(httpContextAccessor.HttpContext?.Request.Query["orderDate"], out var orderDate) ? orderDate : null;
}