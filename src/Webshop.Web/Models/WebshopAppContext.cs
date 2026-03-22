using Webshop.Core.Abstractions;
using Webshop.Core.Constants;

namespace Webshop.Web.Models;

public class WebshopAppContext : IAppContext
{
    public WebshopAppTypes AppType { get; set; }
}