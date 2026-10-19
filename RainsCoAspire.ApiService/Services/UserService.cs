using Microsoft.AspNetCore.Identity;
using RainsCoAspire.ApiService.Dtos;
using RainsCoAspire.ApiService.Models;
using RainsCoAspire.ApiService.Repositories;
using RainsCoAspire.ApiService.Security;

namespace RainsCoAspire.ApiService.Services;

public class UserService(IUserRepository repository, IPasswordHasher<User> passwordHasher) : IUserService
{
    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await repository.GetAllAsync(cancellationToken);
        return users.Select(UserResponse.FromUser).ToList();
    }

    public async Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : UserResponse.FromUser(user);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>();
        await ValidateUsernameAsync(request.Username, excludeId: null, errors, cancellationToken);
        var role = await FindRoleAsync(request.Role, errors, cancellationToken);
        ThrowIfInvalid(errors);

        var user = new User
        {
            Username = request.Username,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = role!,
        };
        passwordHasher.SetPassword(user, request.Password);
        user.CreatedAt = user.UpdatedAt;

        repository.Add(user);
        await repository.SaveChangesAsync(cancellationToken);

        return UserResponse.FromUser(user);
    }

    public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var errors = new Dictionary<string, string[]>();
        await ValidateUsernameAsync(request.Username, excludeId: id, errors, cancellationToken);
        var role = await FindRoleAsync(request.Role, errors, cancellationToken);

        var roleChanged = role is not null && role.Id != user.RoleId;
        if (roleChanged && user.Role.Name == AppRoles.Admin && await repository.CountUsersInRoleAsync(user.RoleId, cancellationToken) == 1)
        {
            errors[nameof(UpdateUserRequest.Role)] = ["This is the only admin. Make another user an admin first."];
        }

        ThrowIfInvalid(errors);

        if (roleChanged)
        {
            user.Role = role!;
            // Tokens carry the role, so the user has to sign in again to get one with the new role.
            user.RenewSecurityStamp();
        }

        user.Username = request.Username;
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);

        return UserResponse.FromUser(user);
    }

    public async Task<bool> DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        // Only admins can delete users, so as long as admins can't delete themselves at least one admin remains.
        if (user.Id == currentUserId)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["id"] = ["You can't delete your own account."],
            });
        }

        repository.Remove(user);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(int id, ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        passwordHasher.SetPassword(user, request.NewPassword);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task ValidateUsernameAsync(string username, int? excludeId, Dictionary<string, string[]> errors, CancellationToken cancellationToken)
    {
        if (await repository.UsernameExistsAsync(username, excludeId, cancellationToken))
        {
            errors[nameof(CreateUserRequest.Username)] = [$"The username '{username}' is already taken."];
        }
    }

    private async Task<Role?> FindRoleAsync(string roleName, Dictionary<string, string[]> errors, CancellationToken cancellationToken)
    {
        var role = await repository.GetRoleByNameAsync(roleName.Trim(), cancellationToken);
        if (role is null)
        {
            errors[nameof(CreateUserRequest.Role)] = [$"The role must be one of: {string.Join(", ", AppRoles.All)}."];
        }

        return role;
    }

    private static void ThrowIfInvalid(Dictionary<string, string[]> errors)
    {
        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }
    }
}
