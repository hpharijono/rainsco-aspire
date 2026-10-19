using RainsCoAspire.ApiService.Models;

namespace RainsCoAspire.ApiService.Services;

public interface ITokenService
{
    // The user's Role must be loaded.
    IssuedToken CreateToken(User user);
}

public sealed record IssuedToken(string Token, DateTimeOffset ExpiresAt);
