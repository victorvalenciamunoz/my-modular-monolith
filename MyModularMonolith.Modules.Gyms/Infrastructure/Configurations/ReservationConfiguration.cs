using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyModularMonolith.Modules.Gyms.Domain;

namespace MyModularMonolith.Modules.Gyms.Infrastructure.Configurations;

internal class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(450); 
        builder.Property(x => x.GymProductId).IsRequired();
        builder.Property(x => x.ReservationDateTime).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.UserNotes).HasMaxLength(500);
        builder.Property(x => x.CancellationReason).HasMaxLength(500);
        builder.Property(x => x.CancelledAt);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        // Relationships
        builder.HasOne(x => x.GymProduct)
               .WithMany()
               .HasForeignKey(x => x.GymProductId)
               .OnDelete(DeleteBehavior.Restrict); 

        // Indexes
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.GymProductId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ReservationDateTime);
        builder.HasIndex(x => x.CreatedAt); 
        builder.HasIndex(x => new { x.UserId, x.Status });
        builder.HasIndex(x => new { x.GymProductId, x.ReservationDateTime, x.Status }); 

        // Configure domain events (ignored)
        builder.Ignore(x => x.DomainEvents);
    }
}