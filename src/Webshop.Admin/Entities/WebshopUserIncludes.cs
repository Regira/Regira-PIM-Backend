namespace Webshop.Admin.Entities;

[Flags]
public enum WebshopUserIncludes
{
    None = 0,
    UserClaims = 1 << 0,
    All = UserClaims
}
