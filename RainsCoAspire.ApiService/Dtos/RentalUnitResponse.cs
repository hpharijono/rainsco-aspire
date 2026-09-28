namespace RainsCoAspire.ApiService.Dtos;

public record RentalUnitResponse(
    int Id,
    string Alias,
    string? Description,
    decimal SquareMeters,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<RentalUnitImageResponse> Images);

// Url is a time-limited pre-signed S3 link.
public record RentalUnitImageResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Url);
