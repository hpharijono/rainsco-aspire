using Microsoft.EntityFrameworkCore;
using RainsCoAspire.ApiService.Models;

namespace RainsCoAspire.ApiService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<RentalUnit> RentalUnits => Set<RentalUnit>();

    public DbSet<RentalUnitImage> RentalUnitImages => Set<RentalUnitImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
