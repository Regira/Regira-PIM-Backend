using Microsoft.AspNetCore.Http;
using Webshop.Core.Abstractions;
using Webshop.Core.Constants;

namespace Webshop.Web.Models;

public class AppContextLoaderMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICultureContext cultureContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var culture = context.User.Claims.FirstOrDefault(c => c.Type == WebshopClaimTypes.Culture)?.Value;

            await cultureContext.Init(culture);
        }

        await next(context);
    }
}