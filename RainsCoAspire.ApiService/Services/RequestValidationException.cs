namespace RainsCoAspire.ApiService.Services;

// Thrown by services when a request fails business-rule validation; controllers turn it into a 400 response.
public class RequestValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("One or more validation errors occurred.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
