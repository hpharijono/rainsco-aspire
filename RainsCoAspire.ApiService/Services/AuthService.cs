using Microsoft.AspNetCore.Identity;
using RainsCoAspire.ApiService.Dtos;
using RainsCoAspire.ApiService.Models;
using RainsCoAspire.ApiService.Repositories;
using RainsCoAspire.ApiService.Security;

namespace RainsCoAspire.ApiService.Services;

public class AuthService(
    IUserRepository repository,
    IPasswordHasher<User> passwordHasher,
    ITokenService tokenService) : IAuthService
{
    // Checked against when the username doesn't exist, so that takes as long as a wrong password and response times
    // don't reveal which usernames exist.
    private static readonly Lazy<string> UnknownUserPasswordHash =
        new(() => new PasswordHasher<User>().HashPassword(new User(), Guid.NewGuid().ToString()));

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByUsernameAsync(request.Username.Trim(), cancellationToken);
        if (user is null)
        {
            passwordHasher.VerifyHashedPassword(new User(), UnknownUserPasswordHash.Value, request.Password);
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            // The hash was made with older, weaker settings. Upgrade it without renewing the security stamp, so the
            // user's other sessions stay signed in.
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            await repository.SaveChangesAsync(cancellationToken);
        }

        return CreateLoginResponse(user);
    }

    public async Task<LoginResponse?> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword) == PasswordVerificationResult.Failed)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                [nameof(ChangePasswordRequest.CurrentPassword)] = ["The current password is incorrect."],
            });
        }

        passwordHasher.SetPassword(user, request.NewPassword);
        await repository.SaveChangesAsync(cancellationToken);

        return CreateLoginResponse(user);
    }

    private LoginResponse CreateLoginResponse(User user)
    {
        var token = tokenService.CreateToken(user);
        return new LoginResponse(token.Token, token.ExpiresAt, UserResponse.FromUser(user));
    }
}
