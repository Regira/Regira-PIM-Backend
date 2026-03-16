using Regira.Office.Mail.Abstractions;

namespace Webshop.Identity.DependencyInjection;

public class WebshopAuthenticationOptions
{
    internal Func<IServiceProvider, IMailer>? MailerFactory;
    public void AddMailer(Func<IServiceProvider, IMailer> factory)
    {
        MailerFactory = factory;
    }
}
public class WebshopIdentityOptions : WebshopAuthenticationOptions
{
    public string? SecretKey { get; set; }
    public List<string> Audiences { get; } = [];
}
