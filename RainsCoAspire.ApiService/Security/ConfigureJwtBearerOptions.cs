using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RainsCoAspire.ApiService.Options;
using RainsCoAspire.ApiService.Repositories;

namespace RainsCoAspire.ApiService.Security;

// Validates the tokens issued by JwtTokenService. Configured here instead of inline in Program.cs so it can use
// the validated JwtOptions.
public class ConfigureJwtBearerOptions(IOptions<JwtOptions> jwtOptions) : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme)
        {
            return;
        }

        var jwt = jwtOptions.Value;

        // Keep claim types as they are in the token ("sub", "role", ...) instead of mapping them to long URIs.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = jwt.CreateSigningKey(),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            NameClaimType = AppClaimTypes.Username,
            RoleClaimType = AppClaimTypes.Role,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = ValidateSecurityStampAsync,
        };
    }

    public void Configure(JwtBearerOptions options) => Configure(Microsoft.Extensions.Options.Options.DefaultName, options);

    // Rejects tokens for deleted users and tokens issued before the user's password or role changed.
    private static async Task ValidateSecurityStampAsync(TokenValidatedContext context)
    {
        var principal = context.Principal!;
        var securityStamp = principal.FindFirstValue(AppClaimTypes.SecurityStamp);
        var users = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

        if (!int.TryParse(principal.FindFirstValue(AppClaimTypes.UserId), NumberStyles.None, CultureInfo.InvariantCulture, out var userId)
            || securityStamp is null
            || !await users.IsSecurityStampValidAsync(userId, securityStamp, context.HttpContext.RequestAborted))
        {
            context.Fail("The token is no longer valid. Sign in again.");
        }
    }
}
