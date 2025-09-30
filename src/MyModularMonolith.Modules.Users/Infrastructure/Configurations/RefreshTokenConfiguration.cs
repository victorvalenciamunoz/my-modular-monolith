using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyModularMonolith.Modules.Users.Domain;

namespace MyModularMonolith.Modules.Users.Infrastructure.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {        
        builder.ToTable("RefreshTokens");
        builder.HasKey(x => x.Id);
             
        builder.Property(x => x.Token).IsRequired().HasMaxLength(500);
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.IsRevoked).IsRequired();
        builder.Property(x => x.RevokedAt);
        builder.Property(x => x.CreatedByIp).IsRequired().HasMaxLength(50);
        builder.Property(x => x.RevokedByIp).HasMaxLength(50);
        builder.Property(x => x.ReplacedByToken).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);
                
        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
                
        builder.HasIndex(x => x.Token).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ExpiresAt);
        builder.HasIndex(x => x.IsRevoked);
    }
}
