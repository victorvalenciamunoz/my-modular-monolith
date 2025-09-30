using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Domain;

namespace MyModularMonolith.Modules.Gyms.Application.Mappers;

internal static class ProductMapper
{
    public static ProductDto ToDto(Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.BasePrice,
            product.RequiresSchedule,
            product.RequiresInstructor,
            product.HasCapacityLimits,
            product.IsActive,
            product.CreatedAt,
            product.UpdatedAt);

    public static GymProductDto ToDto(GymProduct gymProduct) =>
        new(
            gymProduct.Id,
            gymProduct.GymId,
            gymProduct.Gym.Name,
            gymProduct.ProductId,
            gymProduct.Product.Name,
            gymProduct.Product.Description,
            gymProduct.Price,
            gymProduct.DiscountPercentage,
            gymProduct.GetFinalPrice(),
            gymProduct.IsActive,
            gymProduct.Schedule.ToJsonString(), 
            gymProduct.MinCapacity,
            gymProduct.MaxCapacity,
            gymProduct.InstructorName,
            gymProduct.InstructorEmail,
            gymProduct.InstructorPhone,
            gymProduct.Notes,
            gymProduct.Equipment,
            gymProduct.CreatedAt,
            gymProduct.UpdatedAt);

    public static GymProductDto ToDto(GymProduct gymProduct, Gym gym, Product product) =>
        new(
            gymProduct.Id,
            gymProduct.GymId,
            gym.Name,
            gymProduct.ProductId,
            product.Name,
            product.Description,
            gymProduct.Price,
            gymProduct.DiscountPercentage,
            gymProduct.GetFinalPrice(),
            gymProduct.IsActive,
            gymProduct.Schedule.ToJsonString(),
            gymProduct.MinCapacity,
            gymProduct.MaxCapacity,
            gymProduct.InstructorName,
            gymProduct.InstructorEmail,
            gymProduct.InstructorPhone,
            gymProduct.Notes,
            gymProduct.Equipment,
            gymProduct.CreatedAt,
            gymProduct.UpdatedAt);
}
