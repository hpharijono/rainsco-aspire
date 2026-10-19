using Microsoft.EntityFrameworkCore;
using RainsCoAspire.ApiService.Data;
using RainsCoAspire.ApiService.Models;

namespace RainsCoAspire.ApiService.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        dbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .OrderBy(u => u.Username)
            .ToListAsync(cancellationToken);

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public Task<bool> UsernameExistsAsync(string username, int? excludeId = null, CancellationToken cancellationToken = default) =>
        dbContext.Users
            .AnyAsync(u => u.Username == username && (excludeId == null || u.Id != excludeId), cancellationToken);

    public Task<bool> IsSecurityStampValidAsync(int id, string securityStamp, CancellationToken cancellationToken = default) =>
        dbContext.Users
            .AnyAsync(u => u.Id == id && u.SecurityStamp == securityStamp, cancellationToken);

    public Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default) =>
        dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);

    public Task<int> CountUsersInRoleAsync(int roleId, CancellationToken cancellationToken = default) =>
        dbContext.Users
            .CountAsync(u => u.RoleId == roleId, cancellationToken);

    public void Add(User user) => dbContext.Users.Add(user);

    public void Remove(User user) => dbContext.Users.Remove(user);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
