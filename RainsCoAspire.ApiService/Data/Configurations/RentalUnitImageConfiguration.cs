using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainsCoAspire.ApiService.Models;

namespace RainsCoAspire.ApiService.Data.Configurations;

public class RentalUnitImageConfiguration : IEntityTypeConfiguration<RentalUnitImage>
{
    public void Configure(EntityTypeBuilder<RentalUnitImage> builder)
    {
        builder.ToTable("RentalUnitImages");

        // Ids are assigned in code before upload so they can be part of the S3 key.
        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.S3Key)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(i => i.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(i => i.ContentType)
            .HasMaxLength(100)
            .IsRequired();
    }
}
