using RainsCoAspire.ApiService.Dtos;

namespace RainsCoAspire.ApiService.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // Throws RequestValidationException when the username is taken or the role doesn't exist.
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    // Returns null when the user doesn't exist. Throws RequestValidationException when the username is taken, the
    // role doesn't exist, or the change would leave no admins.
    Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);

    // Returns false when the user doesn't exist. Throws RequestValidationException when users try to delete themselves.
    Task<bool> DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default);

    // Returns false when the user doesn't exist. The user is signed out of their existing sessions.
    Task<bool> ResetPasswordAsync(int id, ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
