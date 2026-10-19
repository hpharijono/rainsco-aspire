using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RainsCoAspire.ApiService.Models;
using RainsCoAspire.ApiService.Security;

namespace RainsCoAspire.ApiService.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public const int AdminRoleId = 1;

    public const int RenterRoleId = 2;

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.Property(r => r.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.HasData(
            new Role { Id = AdminRoleId, Name = AppRoles.Admin },
            new Role { Id = RenterRoleId, Name = AppRoles.Renter });
    }
}
