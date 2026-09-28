namespace RainsCoAspire.ApiService.Services;

public interface IImageStorageService
{
    Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);

    Task DeleteAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default);

    Task<string> GetUrlAsync(string key);
}
