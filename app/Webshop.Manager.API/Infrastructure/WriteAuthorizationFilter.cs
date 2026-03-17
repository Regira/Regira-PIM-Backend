using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Webshop.Core.Constants;

namespace Webshop.Manager.API.Infrastructure;

/// <summary>
/// Enforces write-operation authorization:
/// - WebshopUser controller (POST/PUT/DELETE): requires Admin permission
/// - All other controllers (POST/PUT/DELETE): requires Editor or Admin permission
/// Reads pass through — covered by the global RequireAuthorization() on the endpoint mapping.
/// </summary>
public class WriteAuthorizationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        var isWriteOperation = HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsDelete(method);

        if (isWriteOperation)
        {
            var permissions = context.HttpContext.User
                .FindAll(WebshopClaimTypes.Permission)
                .Select(c => c.Value)
                .ToHashSet();

            var controller = context.RouteData.Values["controller"]?.ToString();
            var isUserController = string.Equals(controller, "WebshopUser", StringComparison.OrdinalIgnoreCase);

            var authorized = isUserController
                ? permissions.Contains(WebshopPermissionValues.Admin)
                : permissions.Overlaps([WebshopPermissionValues.Editor, WebshopPermissionValues.Admin]);

            if (!authorized)
            {
                context.Result = new ForbidResult();
                return;
            }
        }

        await next();
    }
}
