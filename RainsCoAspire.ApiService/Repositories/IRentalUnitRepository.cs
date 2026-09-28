using RainsCoAspire.ApiService.Models;

namespace RainsCoAspire.ApiService.Repositories;

public interface IRentalUnitRepository
{
    Task<List<RentalUnit>> GetAllAsync(CancellationToken cancellationToken = default);

    // Returns a tracked entity (with images) so changes can be saved with SaveChangesAsync.
    Task<RentalUnit?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> AliasExistsAsync(string alias, int? excludeId = null, CancellationToken cancellationToken = default);

    void Add(RentalUnit rentalUnit);

    void Remove(RentalUnit rentalUnit);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
