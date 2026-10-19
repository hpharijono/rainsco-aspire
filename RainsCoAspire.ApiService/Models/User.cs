namespace RainsCoAspire.ApiService.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    // Salted PBKDF2 hash from ASP.NET Core's PasswordHasher. The password itself is never stored.
    public string PasswordHash { get; set; } = string.Empty;

    // Replaced whenever the password or role changes. It's a claim in each JWT, so tokens issued before the
    // change stop being accepted.
    public string SecurityStamp { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
