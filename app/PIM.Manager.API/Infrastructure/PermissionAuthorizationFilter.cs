using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PIM.Core.Constants;

namespace PIM.Admin.API.Infrastructure;

/// <summary>
/// Enforces per-controller permission authorization:
/// - PimUser controller: requires Admin permission (all methods)
/// - All other controllers: requires Editor permission (all methods)
/// AccountController and PasswordController are exempt via [AllowAnonymous].
/// </summary>
public class PermissionAuthorizationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Respect [AllowAnonymous] on controllers/actions (e.g. AccountController, PasswordController)
        var hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata
            .Any(m => m is IAllowAnonymous);
        if (hasAllowAnonymous)
        {
            await next();
            return;
        }

        var permissions = context.HttpContext.User
            .FindAll(PimClaimTypes.Permission)
            .Select(c => c.Value)
            .ToHashSet();

        var controller = context.RouteData.Values["controller"]?.ToString();
        var isUserController = string.Equals(controller, "PimUser", StringComparison.OrdinalIgnoreCase);

        var authorized = isUserController
            ? permissions.Contains(PimPermissionValues.Admin)
            : permissions.Contains(PimPermissionValues.Editor);

        if (!authorized)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
