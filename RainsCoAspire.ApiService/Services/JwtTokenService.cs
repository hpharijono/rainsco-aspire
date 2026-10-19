using System.Globalization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RainsCoAspire.ApiService.Models;
using RainsCoAspire.ApiService.Options;
using RainsCoAspire.ApiService.Security;

namespace RainsCoAspire.ApiService.Services;

public class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    private static readonly JsonWebTokenHandler TokenHandler = new();

    public IssuedToken CreateToken(User user)
    {
        var jwt = options.Value;
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(jwt.ExpiryMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(jwt.CreateSigningKey(), SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [AppClaimTypes.UserId] = user.Id.ToString(CultureInfo.InvariantCulture),
                [AppClaimTypes.Username] = user.Username,
                [AppClaimTypes.FirstName] = user.FirstName,
                [AppClaimTypes.LastName] = user.LastName,
                [AppClaimTypes.Role] = user.Role.Name,
                [AppClaimTypes.SecurityStamp] = user.SecurityStamp,
            },
        };

        return new IssuedToken(TokenHandler.CreateToken(descriptor), expiresAt);
    }
}
