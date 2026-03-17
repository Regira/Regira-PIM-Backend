using Microsoft.AspNetCore.Http;
using Regira.Security.Authentication.Jwt.Extensions;
using Webshop.Core.Abstractions;
using Webshop.Core.Constants;

namespace Webshop.Web.Models;

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    protected HttpContext? HttpContext => httpContextAccessor.HttpContext;

    public string? UserId => HttpContext?.User.FindUserId();
    public ICollection<string> Permissions => HttpContext?.User.Claims
                                                  .Where(c => c.Type == WebshopClaimTypes.Permission)
                                                  .Select(c => c.Value).ToList()
                                              ?? [];
}