using RainsCoAspire.ApiService.Dtos;

namespace RainsCoAspire.ApiService.Services;

public interface IRentalUnitService
{
    Task<IReadOnlyList<RentalUnitResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<RentalUnitResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // Throws RequestValidationException when the alias is taken or an image is invalid.
    Task<RentalUnitResponse> CreateAsync(CreateRentalUnitRequest request, CancellationToken cancellationToken = default);

    // Returns null when the rental unit doesn't exist.
    Task<RentalUnitResponse?> UpdateAsync(int id, UpdateRentalUnitRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
