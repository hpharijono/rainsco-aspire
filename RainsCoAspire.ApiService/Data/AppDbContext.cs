using Microsoft.EntityFrameworkCore;
using RainsCoAspire.ApiService.Models;

namespace RainsCoAspire.ApiService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<RentalUnit> RentalUnits => Set<RentalUnit>();

    public DbSet<RentalUnitImage> RentalUnitImages => Set<RentalUnitImage>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
