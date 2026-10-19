using Microsoft.AspNetCore.Identity;
using RainsCoAspire.ApiService.Models;

namespace RainsCoAspire.ApiService.Security;

public static class UserSecurityExtensions
{
    // Also renews the security stamp, which signs the user out of every existing session.
    public static void SetPassword(this IPasswordHasher<User> passwordHasher, User user, string password)
    {
        user.PasswordHash = passwordHasher.HashPassword(user, password);
        user.RenewSecurityStamp();
        user.UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static void RenewSecurityStamp(this User user) => user.SecurityStamp = Guid.NewGuid().ToString();
}
