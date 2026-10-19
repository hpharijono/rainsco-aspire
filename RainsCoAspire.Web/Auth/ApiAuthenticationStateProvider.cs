using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using RainsCoAspire.Web.Models;

namespace RainsCoAspire.Web.Auth;

// Tracks the signed-in user for the current circuit. The API's JWT is kept in the browser's localStorage so the
// session survives reloads and new tabs; when a circuit starts, the stored token is checked with the API.
// Uses JS interop, so it only works in interactive (non-prerendered) components.
public class ApiAuthenticationStateProvider(
    IJSRuntime jsRuntime,
    AuthApiClient authApi,
    ILogger<ApiAuthenticationStateProvider> logger) : AuthenticationStateProvider
{
    private const string TokenStorageKey = "rainsco.authToken";

    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    private Task<AuthenticationState>? authenticationState;

    // Bearer token for API calls; null when signed out.
    public string? AccessToken { get; private set; }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        authenticationState ??= RestoreSessionAsync();

    public async Task SignInAsync(LoginResult login)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenStorageKey, login.Token);
        SetState(login.Token, CreateState(login.User));
    }

    public async Task SignOutAsync()
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenStorageKey);
        SetState(null, Anonymous);
    }

    private async Task<AuthenticationState> RestoreSessionAsync()
    {
        try
        {
            var token = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenStorageKey);
            if (string.IsNullOrEmpty(token))
            {
                return Anonymous;
            }

            var user = await authApi.GetCurrentUserAsync(token);
            if (user is null)
            {
                // Expired, or revoked by a password reset or role change.
                await jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenStorageKey);
                return Anonymous;
            }

            AccessToken = token;
            return CreateState(user);
        }
        catch (Exception ex)
        {
            // E.g. the API is down. Show the sign-in page instead of failing every page. The token stays stored, so
            // the next visit tries it again.
            logger.LogWarning(ex, "Couldn't restore the signed-in session.");
            return Anonymous;
        }
    }

    private void SetState(string? token, AuthenticationState state)
    {
        AccessToken = token;
        authenticationState = Task.FromResult(state);
        NotifyAuthenticationStateChanged(authenticationState);
    }

    private static AuthenticationState CreateState(User user)
    {
        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new(ClaimTypes.Role, user.Role),
        ];

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "ApiJwt")));
    }
}
