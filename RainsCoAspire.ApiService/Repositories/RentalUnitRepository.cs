using Microsoft.EntityFrameworkCore;
using RainsCoAspire.ApiService.Data;
using RainsCoAspire.ApiService.Models;

namespace RainsCoAspire.ApiService.Repositories;

public class RentalUnitRepository(AppDbContext dbContext) : IRentalUnitRepository
{
    public Task<List<RentalUnit>> GetAllAsync(CancellationToken cancellationToken = default) =>
        dbContext.RentalUnits
            .AsNoTracking()
            .Include(u => u.Images)
            .OrderBy(u => u.Alias)
            .ToListAsync(cancellationToken);

    public Task<RentalUnit?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.RentalUnits
            .Include(u => u.Images)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<bool> AliasExistsAsync(string alias, int? excludeId = null, CancellationToken cancellationToken = default) =>
        dbContext.RentalUnits
            .AnyAsync(u => u.Alias == alias && (excludeId == null || u.Id != excludeId), cancellationToken);

    public void Add(RentalUnit rentalUnit) => dbContext.RentalUnits.Add(rentalUnit);

    public void Remove(RentalUnit rentalUnit) => dbContext.RentalUnits.Remove(rentalUnit);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
