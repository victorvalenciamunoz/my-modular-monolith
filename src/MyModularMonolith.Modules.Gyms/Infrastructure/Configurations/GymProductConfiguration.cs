using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.ValueObjects;
using MyModularMonolith.Shared.Domain.ValueObjects;

namespace MyModularMonolith.Modules.Gyms.Infrastructure.Configurations;

internal class GymProductConfiguration : IEntityTypeConfiguration<GymProduct>
{
    public void Configure(EntityTypeBuilder<GymProduct> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.GymId).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();

        builder.Property(x => x.Price)
            .HasConversion(
                money => money.Value,
                value => Money.Create(value))
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.DiscountPercentage).HasColumnType("decimal(5,2)");
        builder.Property(x => x.IsActive).IsRequired();

        builder.Property(x => x.Schedule)
            .HasMaxLength(2000)
            .HasConversion(
                schedule => schedule.ToJsonString(),
                jsonString => WeeklySchedule.FromJsonString(jsonString)
            );

        builder.Property(x => x.MinCapacity);
        builder.Property(x => x.MaxCapacity);
        builder.Property(x => x.InstructorName).HasMaxLength(200);
        builder.Property(x => x.InstructorEmail).HasMaxLength(256);
        builder.Property(x => x.InstructorPhone).HasMaxLength(20);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Equipment).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        builder.HasOne(x => x.Gym)
               .WithMany(x => x.GymProducts)
               .HasForeignKey(x => x.GymId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
               .WithMany(x => x.GymProducts)
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.GymId);
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => new { x.GymId, x.ProductId }).IsUnique();
        builder.HasIndex(x => new { x.GymId, x.IsActive });     
    }
}
