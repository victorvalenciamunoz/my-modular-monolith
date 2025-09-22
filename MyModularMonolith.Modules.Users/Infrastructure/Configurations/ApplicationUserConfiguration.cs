using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyModularMonolith.Modules.Users.Domain;

namespace MyModularMonolith.Modules.Users.Infrastructure.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {        
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.IsActive).IsRequired();
             
        builder.Property(x => x.HomeGymId);
        builder.Property(x => x.HomeGymName).HasMaxLength(200);
        builder.Property(x => x.RegistrationDate);
                
        builder.Property(x => x.HasTemporaryPassword).IsRequired();
        builder.Property(x => x.TemporaryPasswordCreatedAt);
        builder.Property(x => x.MustChangePassword).IsRequired();
                
        builder.HasIndex(x => x.HomeGymId);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.HasTemporaryPassword);
                
        builder.Ignore(x => x.DomainEvents);
    }
}
