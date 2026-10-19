using RainsCoAspire.ApiService.Dtos;

namespace RainsCoAspire.ApiService.Services;

public interface IAuthService
{
    // Returns null when the username or password is wrong.
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    // Returns null when the user no longer exists. Throws RequestValidationException when the current password is
    // wrong. Changing the password invalidates the user's existing tokens, so the response carries a new one.
    Task<LoginResponse?> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
