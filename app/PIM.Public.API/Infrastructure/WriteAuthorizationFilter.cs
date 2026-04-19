using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PIM.Shop.API.Infrastructure;

/// <summary>
/// Central authorization filter for the Public API:
/// - [AllowAnonymous] endpoints (AccountController, PasswordController) → pass through
/// - Product controller → GET/Search/List are public; write operations (Save, Create, Modify, Delete) are forbidden
/// - All other write operations → Forbidden
/// - All other read operations → require authenticated user
/// Note: POST /search and POST /list are treated as reads, not writes.
/// </summary>
public class WriteAuthorizationFilter : IAsyncActionFilter
{
    private static bool IsWriteOperation(ActionExecutingContext context)
    {
        var method = context.HttpContext.Request.Method;
        if (!HttpMethods.IsPost(method) && !HttpMethods.IsPut(method) && !HttpMethods.IsDelete(method))
            return false;

        // POST /search and POST /list are reads despite using POST
        var action = context.RouteData.Values["action"]?.ToString();
        return !string.Equals(action, "Search", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(action, "List", StringComparison.OrdinalIgnoreCase);
    }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Respect [AllowAnonymous] on any controller or action
        var isAnonymousAllowed = context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any();
        if (isAnonymousAllowed)
        {
            await next();
            return;
        }

        var controller = context.RouteData.Values["controller"]?.ToString();

        // Products are publicly readable, but not writable
        if (string.Equals(controller, "Product", StringComparison.OrdinalIgnoreCase))
        {
            if (IsWriteOperation(context))
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
            return;
        }

        var user = context.HttpContext.User;

        // Writes on any other entity are not allowed in the public API
        if (IsWriteOperation(context))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Reads on other entities require an authenticated user
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        await next();
    }
}
