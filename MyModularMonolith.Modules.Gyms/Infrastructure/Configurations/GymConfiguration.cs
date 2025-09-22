using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyModularMonolith.Modules.Gyms.Domain;

namespace MyModularMonolith.Modules.Gyms.Infrastructure.Configurations;

internal class GymConfiguration : IEntityTypeConfiguration<Gym>
{
    public void Configure(EntityTypeBuilder<Gym> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.IsActive).IsRequired();

        // Indexes
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => new { x.Name, x.IsActive });

        // Configure domain events (ignored)
        builder.Ignore(x => x.DomainEvents);

        // Seed data
        SeedData(builder);
    }

    private static void SeedData(EntityTypeBuilder<Gym> builder)
    {
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new
            {
                Id = new Guid("33333333-3333-3333-3333-333333333333"),
                Name = "Gym Central",
                CreatedAt = seedDate,
                IsActive = true
            },
            new
            {
                Id = new Guid("44444444-4444-4444-4444-444444444444"),
                Name = "Gym Norte",
                CreatedAt = seedDate,
                IsActive = true
            },
            new
            {
                Id = new Guid("55555555-5555-5555-5555-555555555555"),
                Name = "Gym Sur",
                CreatedAt = seedDate,
                IsActive = true
            }
        );
    }
}
