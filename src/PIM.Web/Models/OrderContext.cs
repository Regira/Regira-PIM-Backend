using Microsoft.AspNetCore.Http;
using PIM.Core.Abstractions;
using PIM.Core.Constants;

namespace PIM.Web.Models;

public class OrderContext(IHttpContextAccessor httpContextAccessor) : UserContext(httpContextAccessor), IOrderContext
{
    public int? CustomerId => int.TryParse(HttpContext?.User.Claims.FirstOrDefault(c => c.Type == PimClaimTypes.CustomerId)?.Value, out var customerId) ? customerId : null;
    public DateTime? OrderDate => DateTime.TryParse(HttpContext?.Request.Query["orderDate"], out var orderDate) ? orderDate : null;
}