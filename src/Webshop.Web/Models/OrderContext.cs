using Microsoft.AspNetCore.Http;
using Webshop.Core.Abstractions;
using Webshop.Core.Constants;

namespace Webshop.Web.Models;

public class OrderContext(IHttpContextAccessor httpContextAccessor) : UserContext(httpContextAccessor), IOrderContext
{
    public int? CustomerId => int.TryParse(HttpContext?.User.Claims.FirstOrDefault(c => c.Type == WebshopClaimTypes.CustomerId)?.Value, out var customerId) ? customerId : null;
    public DateTime? OrderDate => DateTime.TryParse(HttpContext?.Request.Query["orderDate"], out var orderDate) ? orderDate : null;
}