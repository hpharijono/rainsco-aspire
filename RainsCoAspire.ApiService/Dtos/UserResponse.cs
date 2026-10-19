using RainsCoAspire.ApiService.Models;

namespace RainsCoAspire.ApiService.Dtos;

public record UserResponse(
    int Id,
    string Username,
    string FirstName,
    string LastName,
    string Role,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    // The user's Role must be loaded.
    public static UserResponse FromUser(User user) => new(
        user.Id,
        user.Username,
        user.FirstName,
        user.LastName,
        user.Role.Name,
        user.CreatedAt,
        user.UpdatedAt);
}
