namespace PIM.Identity.Models;

[Flags]
public enum PimUserIncludes
{
    None = 0,
    UserClaims = 1 << 0,
    All = UserClaims
}
