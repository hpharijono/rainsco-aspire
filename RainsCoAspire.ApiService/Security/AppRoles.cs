namespace RainsCoAspire.ApiService.Security;

// Role names as seeded in the Roles table. Use these in [Authorize(Roles = ...)].
public static class AppRoles
{
    public const string Admin = "Admin";

    public const string Renter = "Renter";

    public static readonly IReadOnlyList<string> All = [Admin, Renter];
}
