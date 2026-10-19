using RainsCoAspire.ApiService.Models;

namespace RainsCoAspire.ApiService.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);

    // Returns a tracked entity (with its role) so changes can be saved with SaveChangesAsync.
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // Returns a tracked entity (with its role). Matching is case-insensitive.
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<bool> UsernameExistsAsync(string username, int? excludeId = null, CancellationToken cancellationToken = default);

    // False when the user was deleted or their security stamp changed since the token was issued.
    Task<bool> IsSecurityStampValidAsync(int id, string securityStamp, CancellationToken cancellationToken = default);

    Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<int> CountUsersInRoleAsync(int roleId, CancellationToken cancellationToken = default);

    void Add(User user);

    void Remove(User user);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
