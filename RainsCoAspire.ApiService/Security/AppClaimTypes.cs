using System.Globalization;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace RainsCoAspire.ApiService.Security;

// Claim names used in the JWTs this API issues. Inbound claim mapping is off, so these are also the claim
// types on HttpContext.User.
public static class AppClaimTypes
{
    public const string UserId = JwtRegisteredClaimNames.Sub;

    public const string Username = JwtRegisteredClaimNames.UniqueName;

    public const string FirstName = JwtRegisteredClaimNames.GivenName;

    public const string LastName = JwtRegisteredClaimNames.FamilyName;

    public const string Role = "role";

    public const string SecurityStamp = "stamp";
}

public static class ClaimsPrincipalExtensions
{
    // Only valid on authenticated requests: every token this API issues has a numeric subject.
    public static int GetUserId(this ClaimsPrincipal principal) =>
        int.Parse(principal.FindFirstValue(AppClaimTypes.UserId)!, CultureInfo.InvariantCulture);
}
