using Regira.Office.Mail.Abstractions;

namespace PIM.Identity.DependencyInjection;

public class PimAuthenticationOptions
{
    internal Func<IServiceProvider, IMailService>? MailerFactory;
    public void AddMailer(Func<IServiceProvider, IMailService> factory)
    {
        MailerFactory = factory;
    }
}
public class PimIdentityOptions : PimAuthenticationOptions
{
    public string? SecretKey { get; set; }
    public List<string> Audiences { get; } = [];
}
