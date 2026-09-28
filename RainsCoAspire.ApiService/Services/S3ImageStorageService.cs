using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using RainsCoAspire.ApiService.Options;

namespace RainsCoAspire.ApiService.Services;

public class S3ImageStorageService(
    IServiceProvider serviceProvider,
    ISecretsProvider secretsProvider,
    IOptions<S3StorageOptions> options) : IImageStorageService
{
    private readonly S3StorageOptions _options = options.Value;

    // Resolved on first use: creating the client throws when no AWS credentials are available, and
    // requests that don't touch images (e.g. listing units without photos) shouldn't depend on S3.
    private IAmazonS3 S3Client => serviceProvider.GetRequiredService<IAmazonS3>();

    public async Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = await GetBucketNameAsync(cancellationToken),
            Key = key,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false,
        };

        await S3Client.PutObjectAsync(request, cancellationToken);
    }

    public async Task DeleteAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default)
    {
        if (keys.Count == 0)
        {
            return;
        }

        var bucketName = await GetBucketNameAsync(cancellationToken);

        // DeleteObjects accepts at most 1000 keys per request.
        foreach (var batch in keys.Chunk(1000))
        {
            var request = new DeleteObjectsRequest
            {
                BucketName = bucketName,
                Objects = batch.Select(key => new KeyVersion { Key = key }).ToList(),
            };

            await S3Client.DeleteObjectsAsync(request, cancellationToken);
        }
    }

    public async Task<string> GetUrlAsync(string key) =>
        await S3Client.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = await GetBucketNameAsync(),
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(_options.PresignedUrlExpiryMinutes),
        });

    private Task<string> GetBucketNameAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.BucketNameSecretId))
        {
            throw new InvalidOperationException(
                $"No Secrets Manager secret is configured for the S3 bucket name. Set '{S3StorageOptions.SectionName}:BucketNameSecretId' in appsettings.");
        }

        return secretsProvider.GetSecretAsync(_options.BucketNameSecretId, _options.BucketNameSecretKey, cancellationToken);
    }
}
