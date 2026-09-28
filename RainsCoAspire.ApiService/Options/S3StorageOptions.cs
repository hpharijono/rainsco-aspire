namespace RainsCoAspire.ApiService.Options;

public class S3StorageOptions
{
    public const string SectionName = "AWS:S3";

    public string BucketName { get; set; } = string.Empty;

    // How long the image URLs handed to clients stay valid. The bucket itself stays private.
    public int PresignedUrlExpiryMinutes { get; set; } = 60;
}
