namespace RainsCoAspire.ApiService.Models;

// Metadata for an image stored in S3. The file itself lives in the bucket under S3Key.
public class RentalUnitImage
{
    public Guid Id { get; set; }

    public int RentalUnitId { get; set; }

    public RentalUnit RentalUnit { get; set; } = null!;

    public string S3Key { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public DateTimeOffset UploadedAt { get; set; }
}
