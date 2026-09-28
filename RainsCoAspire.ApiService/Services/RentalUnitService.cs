using RainsCoAspire.ApiService.Dtos;
using RainsCoAspire.ApiService.Models;
using RainsCoAspire.ApiService.Repositories;
using RainsCoAspire.ApiService.Validation;

namespace RainsCoAspire.ApiService.Services;

public class RentalUnitService(
    IRentalUnitRepository repository,
    IImageStorageService imageStorage,
    ILogger<RentalUnitService> logger) : IRentalUnitService
{
    private const string ImageKeyPrefix = "rental-units";

    public async Task<IReadOnlyList<RentalUnitResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var units = await repository.GetAllAsync(cancellationToken);

        var responses = new List<RentalUnitResponse>(units.Count);
        foreach (var unit in units)
        {
            responses.Add(await ToResponseAsync(unit));
        }

        return responses;
    }

    public async Task<RentalUnitResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var unit = await repository.GetByIdAsync(id, cancellationToken);
        return unit is null ? null : await ToResponseAsync(unit);
    }

    public async Task<RentalUnitResponse> CreateAsync(CreateRentalUnitRequest request, CancellationToken cancellationToken = default)
    {
        var alias = request.Alias.Trim();

        var errors = new Dictionary<string, string[]>();
        await ValidateAliasAsync(alias, excludeId: null, errors, cancellationToken);
        var images = await ValidateImagesAsync(request.Images, errors, cancellationToken);
        ThrowIfInvalid(errors);

        var now = DateTimeOffset.UtcNow;
        var unit = new RentalUnit
        {
            Alias = alias,
            Description = NormalizeDescription(request.Description),
            SquareMeters = request.SquareMeters,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var uploadedKeys = await UploadImagesAsync(unit, images, cancellationToken);
        try
        {
            repository.Add(unit);
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Don't leave orphaned files in S3 when the database write fails.
            await DeleteFromStorageAsync(uploadedKeys);
            throw;
        }

        return await ToResponseAsync(unit);
    }

    public async Task<RentalUnitResponse?> UpdateAsync(int id, UpdateRentalUnitRequest request, CancellationToken cancellationToken = default)
    {
        var unit = await repository.GetByIdAsync(id, cancellationToken);
        if (unit is null)
        {
            return null;
        }

        var alias = request.Alias.Trim();
        var removeIds = request.RemoveImageIds?.ToHashSet() ?? [];

        var errors = new Dictionary<string, string[]>();
        await ValidateAliasAsync(alias, excludeId: id, errors, cancellationToken);

        var unknownIds = removeIds.Where(imageId => unit.Images.All(i => i.Id != imageId)).ToList();
        if (unknownIds.Count > 0)
        {
            errors[nameof(UpdateRentalUnitRequest.RemoveImageIds)] =
                [$"Rental unit {id} has no image with id {string.Join(", ", unknownIds)}."];
        }

        var images = await ValidateImagesAsync(request.Images, errors, cancellationToken);
        ThrowIfInvalid(errors);

        unit.Alias = alias;
        unit.Description = NormalizeDescription(request.Description);
        unit.SquareMeters = request.SquareMeters;
        unit.UpdatedAt = DateTimeOffset.UtcNow;

        // Removing an image from the collection deletes its row, since an image can't exist without its rental unit.
        var removedImages = unit.Images.Where(i => removeIds.Contains(i.Id)).ToList();
        foreach (var image in removedImages)
        {
            unit.Images.Remove(image);
        }

        var uploadedKeys = await UploadImagesAsync(unit, images, cancellationToken);
        try
        {
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await DeleteFromStorageAsync(uploadedKeys);
            throw;
        }

        // Only delete the files once the database no longer references them.
        await DeleteFromStorageAsync(removedImages.Select(i => i.S3Key).ToList());

        return await ToResponseAsync(unit);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var unit = await repository.GetByIdAsync(id, cancellationToken);
        if (unit is null)
        {
            return false;
        }

        var imageKeys = unit.Images.Select(i => i.S3Key).ToList();

        // Image rows are removed by the cascade delete.
        repository.Remove(unit);
        await repository.SaveChangesAsync(cancellationToken);

        await DeleteFromStorageAsync(imageKeys);

        return true;
    }

    private async Task ValidateAliasAsync(string alias, int? excludeId, Dictionary<string, string[]> errors, CancellationToken cancellationToken)
    {
        if (await repository.AliasExistsAsync(alias, excludeId, cancellationToken))
        {
            errors[nameof(CreateRentalUnitRequest.Alias)] = [$"A rental unit with the alias '{alias}' already exists."];
        }
    }

    private static async Task<List<ValidatedImage>> ValidateImagesAsync(
        List<IFormFile>? files,
        Dictionary<string, string[]> errors,
        CancellationToken cancellationToken)
    {
        var validated = new List<ValidatedImage>();
        var messages = new List<string>();

        foreach (var file in files ?? [])
        {
            var (format, error) = await ImageFileValidator.ValidateAsync(file, cancellationToken);
            if (format is null)
            {
                messages.Add($"{file.FileName}: {error}");
            }
            else
            {
                validated.Add(new ValidatedImage(file, format));
            }
        }

        if (messages.Count > 0)
        {
            errors[nameof(CreateRentalUnitRequest.Images)] = [.. messages];
        }

        return validated;
    }

    private static void ThrowIfInvalid(Dictionary<string, string[]> errors)
    {
        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }
    }

    // Uploads each image to S3 and attaches it to the unit. Returns the uploaded keys so the caller can
    // clean up if saving to the database fails.
    private async Task<List<string>> UploadImagesAsync(RentalUnit unit, List<ValidatedImage> images, CancellationToken cancellationToken)
    {
        var uploadedKeys = new List<string>(images.Count);
        try
        {
            foreach (var (file, format) in images)
            {
                var imageId = Guid.NewGuid();
                var key = $"{ImageKeyPrefix}/{imageId:N}{format.Extension}";

                await using (var stream = file.OpenReadStream())
                {
                    await imageStorage.UploadAsync(key, stream, format.ContentType, cancellationToken);
                }

                uploadedKeys.Add(key);
                unit.Images.Add(new RentalUnitImage
                {
                    Id = imageId,
                    S3Key = key,
                    FileName = GetSafeFileName(file.FileName, format),
                    ContentType = format.ContentType,
                    SizeBytes = file.Length,
                    UploadedAt = DateTimeOffset.UtcNow,
                });
            }
        }
        catch
        {
            await DeleteFromStorageAsync(uploadedKeys);
            throw;
        }

        return uploadedKeys;
    }

    // Best effort: a failure leaves an orphaned file in S3 but shouldn't fail a request whose data is already saved.
    private async Task DeleteFromStorageAsync(IReadOnlyCollection<string> keys)
    {
        if (keys.Count == 0)
        {
            return;
        }

        try
        {
            await imageStorage.DeleteAsync(keys, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete {Count} image(s) from S3: {Keys}", keys.Count, keys);
        }
    }

    private async Task<RentalUnitResponse> ToResponseAsync(RentalUnit unit)
    {
        var images = new List<RentalUnitImageResponse>(unit.Images.Count);
        foreach (var image in unit.Images.OrderBy(i => i.UploadedAt))
        {
            var url = await imageStorage.GetUrlAsync(image.S3Key);
            images.Add(new RentalUnitImageResponse(image.Id, image.FileName, image.ContentType, image.SizeBytes, url));
        }

        return new RentalUnitResponse(
            unit.Id,
            unit.Alias,
            unit.Description,
            unit.SquareMeters,
            unit.CreatedAt,
            unit.UpdatedAt,
            images);
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static string GetSafeFileName(string fileName, ImageFormat format)
    {
        var name = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(name))
        {
            return $"image{format.Extension}";
        }

        return name.Length <= 255 ? name : name[^255..];
    }

    private sealed record ValidatedImage(IFormFile File, ImageFormat Format);
}
