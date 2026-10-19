using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainsCoAspire.ApiService.Models;

namespace RainsCoAspire.ApiService.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.Property(u => u.Username)
            .HasMaxLength(50)
            .IsRequired();

        // The default SQL Server collation is case-insensitive, so "Admin" and "admin" count as the same username.
        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.SecurityStamp)
            .HasMaxLength(64)
            .IsRequired();

        // A role can't be deleted while users still have it.
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed data has to be constant, so the hash is precomputed. It's the PasswordHasher hash of "admin123".
        // Change this password after the first sign-in.
        var seededAt = new DateTimeOffset(2026, 10, 18, 0, 0, 0, TimeSpan.Zero);
        builder.HasData(new User
        {
            Id = 1,
            Username = "admin",
            FirstName = "System",
            LastName = "Administrator",
            PasswordHash = "AQAAAAIAAYagAAAAEJVfHdFpfc3g26mI4pABENl4UFBnknSJ/n6uu7y42G8FfW59UU8mDQM1b3i+irVkBg==",
            SecurityStamp = "58822e79-bdb4-429e-b172-4b4843f2bb8d",
            RoleId = RoleConfiguration.AdminRoleId,
            CreatedAt = seededAt,
            UpdatedAt = seededAt,
        });
    }
}
