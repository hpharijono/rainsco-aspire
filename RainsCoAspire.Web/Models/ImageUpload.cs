namespace RainsCoAspire.Web.Models;

// An image picked in the browser, read into memory so it survives the user picking more files.
public sealed record ImageUpload(string FileName, string ContentType, byte[] Data);

public static class ImageUploadRules
{
    // Keep in sync with the API's ImageFileValidator.
    public const long MaxFileSizeBytes = 3 * 1024 * 1024;

    public const int MaxFilesPerSelection = 10;

    public static readonly IReadOnlySet<string> AllowedContentTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp", "image/gif" };

    public static readonly string AcceptAttribute = string.Join(',', AllowedContentTypes);
}
