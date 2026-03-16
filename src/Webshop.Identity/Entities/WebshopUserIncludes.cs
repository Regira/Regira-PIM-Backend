namespace Webshop.Identity.Entities;

[Flags]
public enum WebshopUserIncludes
{
    None = 0,
    UserClaims = 1 << 0,
    All = UserClaims
}
