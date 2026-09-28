namespace RainsCoAspire.Web.Models;

public record RentalUnit(
    int Id,
    string Alias,
    string? Description,
    decimal SquareMeters,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<RentalUnitImage> Images);

// Url is a time-limited pre-signed S3 link.
public record RentalUnitImage(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Url);
