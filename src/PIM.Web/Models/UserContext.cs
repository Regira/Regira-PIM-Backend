using Microsoft.AspNetCore.Http;
using PIM.Core.Abstractions;
using PIM.Core.Constants;
using Regira.Security.Authentication.Jwt.Extensions;

namespace PIM.Web.Models;

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    protected HttpContext? HttpContext => httpContextAccessor.HttpContext;

    public string? UserId => HttpContext?.User.FindUserId();
    public ICollection<string> Permissions => HttpContext?.User.Claims
                                                  .Where(c => c.Type == PimClaimTypes.Permission)
                                                  .Select(c => c.Value).ToList()
                                              ?? [];
}