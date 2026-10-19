using System.ComponentModel.DataAnnotations;

namespace RainsCoAspire.ApiService.Dtos;

public class CreateUserRequest
{
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9._@-]+$", ErrorMessage = "The username can only contain letters, digits and the characters . _ @ -")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    // "Admin" or "Renter".
    [Required]
    public string Role { get; set; } = string.Empty;
}

// Passwords are changed through the reset-password endpoint instead.
public class UpdateUserRequest
{
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9._@-]+$", ErrorMessage = "The username can only contain letters, digits and the characters . _ @ -")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    // "Admin" or "Renter".
    [Required]
    public string Role { get; set; } = string.Empty;
}

// An admin setting a new password for a user.
public class ResetPasswordRequest
{
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}
