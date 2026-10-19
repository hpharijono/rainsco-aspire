using System.Net;
using System.Net.Http.Headers;
using RainsCoAspire.Web.Models;

namespace RainsCoAspire.Web;

public class AuthApiClient(HttpClient httpClient)
{
    private const string BasePath = "/api/auth";

    // Returns null when the username or password is wrong.
    public async Task<LoginResult?> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync($"{BasePath}/login", new { username, password }, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginResult>(cancellationToken)
            ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    // Returns null when the token has expired or was revoked.
    public async Task<User?> GetCurrentUserAsync(string token, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BasePath}/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>(cancellationToken);
    }
}
