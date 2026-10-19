using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RainsCoAspire.ApiService.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    // HMAC-SHA256 key shared by token creation and validation. Don't put it in appsettings: the AppHost generates
    // one, keeps it in its user secrets and passes it in as Jwt__SigningKey.
    [Required(ErrorMessage = "Jwt:SigningKey is not set. Run the app through the AppHost, or set it with dotnet user-secrets.")]
    [MinLength(32, ErrorMessage = "Jwt:SigningKey must be at least 32 characters.")]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpiryMinutes { get; set; } = 60;

    public SymmetricSecurityKey CreateSigningKey() => new(Encoding.UTF8.GetBytes(SigningKey));
}
