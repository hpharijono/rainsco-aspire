using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainsCoAspire.ApiService.Models;

namespace RainsCoAspire.ApiService.Data.Configurations;

public class RentalUnitConfiguration : IEntityTypeConfiguration<RentalUnit>
{
    public void Configure(EntityTypeBuilder<RentalUnit> builder)
    {
        builder.ToTable("RentalUnits");

        builder.Property(u => u.Alias)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(u => u.Alias)
            .IsUnique();

        builder.Property(u => u.Description)
            .HasMaxLength(2000);

        builder.Property(u => u.SquareMeters)
            .HasPrecision(10, 2);

        builder.HasMany(u => u.Images)
            .WithOne(i => i.RentalUnit)
            .HasForeignKey(i => i.RentalUnitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
