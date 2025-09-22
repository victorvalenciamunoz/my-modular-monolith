using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Shared.Domain.ValueObjects;

namespace MyModularMonolith.Modules.Gyms.Infrastructure.Configurations
{
    internal class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).IsRequired().HasMaxLength(1000);            
                        
            builder.Property(x => x.BasePrice)
                .HasConversion(
                    money => money.Value,
                    value => Money.Create(value))
                .IsRequired()
                .HasColumnType("decimal(10,2)");
                
            builder.Property(x => x.RequiresSchedule).IsRequired();
            builder.Property(x => x.RequiresInstructor).IsRequired();
            builder.Property(x => x.HasCapacityLimits).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt);
                        
            builder.HasIndex(x => x.Name);            
            builder.HasIndex(x => x.IsActive);            
                        
            builder.Ignore(x => x.DomainEvents);
            
        }
    }
}
