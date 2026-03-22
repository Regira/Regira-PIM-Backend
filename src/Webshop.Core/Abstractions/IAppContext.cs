using Webshop.Core.Constants;

namespace Webshop.Core.Abstractions;

public interface IAppContext
{
    public WebshopAppTypes AppType { get; set; }
}