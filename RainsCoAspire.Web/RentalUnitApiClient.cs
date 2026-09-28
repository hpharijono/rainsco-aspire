using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using RainsCoAspire.Web.Models;

namespace RainsCoAspire.Web;

public class RentalUnitApiClient(HttpClient httpClient)
{
    private const string BasePath = "/api/rental-units";

    public async Task<RentalUnit[]> GetAllAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<RentalUnit[]>(BasePath, cancellationToken) ?? [];

    public async Task<RentalUnit?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"{BasePath}/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RentalUnit>(cancellationToken);
    }

    public async Task<RentalUnit> CreateAsync(
        RentalUnitFormModel model,
        IReadOnlyList<ImageUpload> images,
        CancellationToken cancellationToken = default)
    {
        using var content = CreateFormContent(model, images, removedImageIds: []);
        using var response = await httpClient.PostAsync(BasePath, content, cancellationToken);
        return await ReadRentalUnitAsync(response, cancellationToken);
    }

    // Returns null when the rental unit no longer exists.
    public async Task<RentalUnit?> UpdateAsync(
        int id,
        RentalUnitFormModel model,
        IReadOnlyList<ImageUpload> newImages,
        IReadOnlyCollection<Guid> removedImageIds,
        CancellationToken cancellationToken = default)
    {
        using var content = CreateFormContent(model, newImages, removedImageIds);
        using var response = await httpClient.PutAsync($"{BasePath}/{id}", content, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        return await ReadRentalUnitAsync(response, cancellationToken);
    }

    // Returns false when the rental unit no longer exists.
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync($"{BasePath}/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    private static MultipartFormDataContent CreateFormContent(
        RentalUnitFormModel model,
        IReadOnlyList<ImageUpload> images,
        IReadOnlyCollection<Guid> removedImageIds)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(model.Alias), nameof(model.Alias) },
            // The API parses numbers using the invariant culture.
            { new StringContent(model.SquareMeters?.ToString(CultureInfo.InvariantCulture) ?? string.Empty), nameof(model.SquareMeters) },
        };

        if (!string.IsNullOrWhiteSpace(model.Description))
        {
            content.Add(new StringContent(model.Description), nameof(model.Description));
        }

        foreach (var image in images)
        {
            var file = new ByteArrayContent(image.Data);
            file.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
            content.Add(file, "Images", image.FileName);
        }

        foreach (var imageId in removedImageIds)
        {
            content.Add(new StringContent(imageId.ToString()), "RemoveImageIds");
        }

        return content;
    }

    private static async Task<RentalUnit> ReadRentalUnitAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(cancellationToken);
            var errors = problem?.Errors.SelectMany(e => e.Value).ToArray();
            throw new ApiValidationException(errors is { Length: > 0 } ? errors : [problem?.Title ?? "The request was invalid."]);
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RentalUnit>(cancellationToken)
            ?? throw new InvalidOperationException("The API returned an empty response.");
    }
}

// The API rejected the request (400). Errors are user-facing messages.
public class ApiValidationException(IReadOnlyList<string> errors) : Exception(string.Join(" ", errors))
{
    public IReadOnlyList<string> Errors { get; } = errors;
}
