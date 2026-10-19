using System.ComponentModel.DataAnnotations;

namespace RainsCoAspire.ApiService.Dtos;

public class LoginRequest
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;
}

// A signed-in user changing their own password.
public class ChangePasswordRequest
{
    [Required]
    [StringLength(100)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}

// Token is a JWT to send as "Authorization: Bearer {token}" until ExpiresAt.
public record LoginResponse(string Token, DateTimeOffset ExpiresAt, UserResponse User);
