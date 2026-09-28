namespace RainsCoAspire.ApiService.Options;

public class S3StorageOptions
{
    public const string SectionName = "AWS:S3";

    // Name or ARN of the Secrets Manager secret that holds the bucket name.
    public string BucketNameSecretId { get; set; } = string.Empty;

    // Key of the bucket name within a key/value secret. Leave empty when the secret is plaintext (just the bucket name).
    public string? BucketNameSecretKey { get; set; } = "BucketName";

    // How long the image URLs handed to clients stay valid. The bucket itself stays private.
    public int PresignedUrlExpiryMinutes { get; set; } = 60;
}
