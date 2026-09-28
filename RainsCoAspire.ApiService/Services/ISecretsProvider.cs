namespace RainsCoAspire.ApiService.Services;

public interface ISecretsProvider
{
    // Returns the secret's plaintext value, or a single value from a key/value (JSON) secret when key is given.
    Task<string> GetSecretAsync(string secretId, string? key = null, CancellationToken cancellationToken = default);
}
