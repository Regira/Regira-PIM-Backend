using Microsoft.AspNetCore.Http;
using PIM.Core.Abstractions;
using PIM.Core.Constants;

namespace PIM.Web.Models;

public class AppContextLoaderMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICultureContext cultureContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var culture = context.User.Claims.FirstOrDefault(c => c.Type == PimClaimTypes.Culture)?.Value;

            await cultureContext.Init(culture);
        }

        await next(context);
    }
}