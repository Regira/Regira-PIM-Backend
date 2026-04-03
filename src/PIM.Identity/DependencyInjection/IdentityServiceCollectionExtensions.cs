using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PIM.Identity.Data;
using PIM.Identity.Models;
using PIM.Identity.Services;
using Regira.Office.Mail.Abstractions;
using Regira.Security.Authentication.Jwt.Extensions;
using Regira.Security.Encryption;

namespace PIM.Identity.DependencyInjection;

public class IdentityServiceBuilder(IServiceCollection services, IdentityBuilder builder, PimIdentityOptions options)
{
    public IServiceCollection Services => services;
    public IdentityBuilder Builder => builder;
    public PimIdentityOptions Options => options;
}
public static class IdentityServiceCollectionExtensions
{
    const string AUTH_SECRET = "PIM_SECRET:F061E1B7-363F-40F3-B052-0EE536792551:5A296568-C287-412F-836B-8E29ACD93A6A";

    public static IdentityServiceBuilder AddPimAuthentication(this IServiceCollection services, IConfiguration config, Func<IServiceProvider, IMailer> mailerFactory)
    {
        return services
            .AddPimAuthentication(o =>
            {
                var options = config.GetSection("Identity").Get<PimIdentityOptions>()!;
                o.SecretKey = options.SecretKey;
                o.Audiences.AddRange(options.Audiences);
                o.AddMailer(mailerFactory);
            });
    }

    public static IdentityServiceBuilder AddPimAuthentication(this IServiceCollection services, Action<PimIdentityOptions> configure)
    {
        var options = new PimIdentityOptions();
        configure.Invoke(options);

        var builder = services
             .AddIdentityCore<PimIdentityUser>(o =>
             {
                 o.SignIn.RequireConfirmedAccount = false;
                 o.User.RequireUniqueEmail = true;
                 o.Password.RequireDigit = false;
                 o.Password.RequireLowercase = false;
                 o.Password.RequireUppercase = false;
                 o.Password.RequireNonAlphanumeric = false;
                 o.Password.RequiredUniqueChars = 1;
                 o.Password.RequiredLength = 2;
                 o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                 o.Lockout.MaxFailedAccessAttempts = 5;
             })
             .AddRoles<IdentityRole>()
             .AddEntityFrameworkStores<AccountsDbContext>()
             .AddUserManager<UserManager<PimIdentityUser>>()
             .AddClaimsPrincipalFactory<PimUserClaimsPrincipalFactory>()
             .AddDefaultTokenProviders();

        if (options.MailerFactory != null)
        {
            services.AddTransient(options.MailerFactory);
            services.AddTransient<IEmailSender, IdentityMailer>();
        }

        return new IdentityServiceBuilder(services, builder, options);
    }

    public static AuthenticationBuilder WithJwtAuthentication(this IdentityServiceBuilder builder)
    {
        var options = builder.Options;

        return builder.Services
            .AddJwtAuthentication(c =>
            {
                var encrypter = new SymmetricEncrypter();
                var secretKey = options.SecretKey;
                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    throw new NullReferenceException("No SecretKey found in Identity section");
                }

                var decryptedKey = encrypter.Decrypt(secretKey);
                var jwtSecret = $"{AUTH_SECRET}:{decryptedKey}";

                c.Secret = jwtSecret;
                c.Authority = "identity-api";
                c.Audiences = options.Audiences;
            });
    }
}