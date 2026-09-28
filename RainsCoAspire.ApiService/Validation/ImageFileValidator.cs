namespace RainsCoAspire.ApiService.Validation;

public sealed record ImageFormat(string ContentType, string Extension);

public static class ImageFileValidator
{
    public const long MaxFileSizeBytes = 3 * 1024 * 1024;

    private const string AllowedFormats = "JPEG, PNG, WebP or GIF";

    private static readonly ImageFormat Jpeg = new("image/jpeg", ".jpg");
    private static readonly ImageFormat Png = new("image/png", ".png");
    private static readonly ImageFormat Gif = new("image/gif", ".gif");
    private static readonly ImageFormat Webp = new("image/webp", ".webp");

    private static ReadOnlySpan<byte> JpegSignature => [0xFF, 0xD8, 0xFF];
    private static ReadOnlySpan<byte> PngSignature => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    // Detects the format from the file's leading bytes instead of trusting the client-supplied content type.
    public static async Task<(ImageFormat? Format, string? Error)> ValidateAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
        {
            return (null, "The file is empty.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return (null, $"The file is {file.Length / (1024d * 1024):0.##} MB; the maximum is {MaxFileSizeBytes / (1024 * 1024)} MB.");
        }

        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);

        var format = DetectFormat(header.AsSpan(0, bytesRead));
        return format is null
            ? (null, $"Only {AllowedFormats} images are allowed.")
            : (format, null);
    }

    private static ImageFormat? DetectFormat(ReadOnlySpan<byte> header)
    {
        if (header.StartsWith(JpegSignature))
        {
            return Jpeg;
        }

        if (header.StartsWith(PngSignature))
        {
            return Png;
        }

        if (header.StartsWith("GIF87a"u8) || header.StartsWith("GIF89a"u8))
        {
            return Gif;
        }

        if (header.Length >= 12 && header.StartsWith("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8))
        {
            return Webp;
        }

        return null;
    }
}
