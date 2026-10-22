using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using PIM.Core.Constants;
using PIM.Identity.Controllers;
using PIM.Manager.API.Controllers;
using PIM.Web.Controllers;

namespace PIM.Manager.API.Infrastructure;

/// <summary>
/// Fails closed: a request passes only through a controller listed in <see cref="ControllerPermissions"/>, for a caller
/// holding one of the permissions its entry names; a controller without an entry refuses every request and logs which one it is.
/// - PimUser controller: requires Admin permission (all methods)
/// - the catalogue, taxonomy and stakeholder controllers: require Editor permission (all methods)
/// - account, password and profile: any signed-in manager (their own session, password and personal data)
/// Keyed on the controller TYPE: a renamed controller fails to compile instead of losing its gate.
/// It is an authorization filter, so it runs before model binding: a refused request gets its 403 whatever its body holds.
/// [AllowAnonymous] counts only on the action itself (sign-in, token refresh, password recovery and reset):
/// on a controller it would open every action, the ones that need a signed-in user included.
/// </summary>
public class PermissionAuthorizationFilter(ILogger<PermissionAuthorizationFilter> logger) : IAsyncAuthorizationFilter
{
    static readonly string[] AnyManager = [];
    static readonly Dictionary<Type, string[]> ControllerPermissions = new()
    {
        [typeof(PimUserController)] = [PimPermissionValues.Admin],

        [typeof(ProductController)] = [PimPermissionValues.Editor],
        [typeof(UnitTypeController)] = [PimPermissionValues.Editor],
        [typeof(FacetController)] = [PimPermissionValues.Editor],
        [typeof(FacetGroupController)] = [PimPermissionValues.Editor],
        [typeof(PartyController)] = [PimPermissionValues.Editor],
        [typeof(RelationshipTypeController)] = [PimPermissionValues.Editor],
        [typeof(CountryController)] = [PimPermissionValues.Editor],

        [typeof(AccountController)] = AnyManager,
        [typeof(PasswordController)] = AnyManager,
        [typeof(ProfileController)] = AnyManager,
    };

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var action = context.ActionDescriptor as ControllerActionDescriptor;
        var isPublic = action?.MethodInfo.GetCustomAttributes(inherit: true).OfType<IAllowAnonymous>().Any() == true;
        if (isPublic)
        {
            return Task.CompletedTask;
        }

        if (action == null || !ControllerPermissions.TryGetValue(action.ControllerTypeInfo.AsType(), out var permissions))
        {
            logger.LogWarning("{Filter} has no entry for {Controller}; its requests are refused.",
                nameof(PermissionAuthorizationFilter), action?.ControllerTypeInfo.Name ?? context.ActionDescriptor.DisplayName);
            context.Result = new ForbidResult();
        }
        else if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
        }
        else if (permissions.Length > 0
                 && !context.HttpContext.User.HasClaim(c => c.Type == PimClaimTypes.Permission && permissions.Contains(c.Value)))
        {
            context.Result = new ForbidResult();
        }

        return Task.CompletedTask;
    }
}
