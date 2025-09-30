using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyModularMonolith.Modules.Users.Domain;

namespace MyModularMonolith.Modules.Users.Infrastructure.Configurations;

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.Property(x => x.CreatedAt).IsRequired();
                    
        SeedData(builder);
    }

    private static void SeedData(EntityTypeBuilder<ApplicationRole> builder)
    {
        var superAdminId = new Guid("11111111-1111-1111-1111-111111111111");
        var userId = new Guid("22222222-2222-2222-2222-222222222222");
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new ApplicationRole(
                UserRoles.SuperAdmin,
                UserRoles.RoleDescriptions[UserRoles.SuperAdmin],
                seedDate)
            {
                Id = superAdminId,
                ConcurrencyStamp = "11111111-1111-1111-1111-111111111111"
            },
            new ApplicationRole(
                UserRoles.User,
                UserRoles.RoleDescriptions[UserRoles.User],
                seedDate)
            {
                Id = userId,
                ConcurrencyStamp = "22222222-2222-2222-2222-222222222222"
            }
        );
    }
}
