using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Caching.Memory;

namespace RainsCoAspire.ApiService.Services;

public class AwsSecretsManagerProvider(IServiceProvider serviceProvider, IMemoryCache cache) : ISecretsProvider
{
    // Secrets rarely change, so avoid a Secrets Manager call on every request. Restart the API to pick up a change sooner.
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    // Resolved on first use: creating the client throws when no AWS credentials are available, and
    // requests that don't need a secret shouldn't depend on AWS.
    private IAmazonSecretsManager SecretsManager => serviceProvider.GetRequiredService<IAmazonSecretsManager>();

    public async Task<string> GetSecretAsync(string secretId, string? key = null, CancellationToken cancellationToken = default)
    {
        var secret = await cache.GetOrCreateAsync($"aws-secret:{secretId}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var response = await SecretsManager.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretId }, cancellationToken);
            return response.SecretString
                ?? throw new InvalidOperationException($"Secret '{secretId}' has no string value.");
        });

        if (string.IsNullOrEmpty(key))
        {
            return secret!;
        }

        // Key/value secrets created in the AWS console are stored as a JSON object.
        try
        {
            using var document = JsonDocument.Parse(secret!);
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty(key, out var value)
                && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString()!;
            }
        }
        catch (JsonException)
        {
            // Plaintext secret; handled below.
        }

        throw new InvalidOperationException(
            $"Secret '{secretId}' has no '{key}' key. Store it as a key/value secret with that key, or clear the key setting to use a plaintext secret.");
    }
}
