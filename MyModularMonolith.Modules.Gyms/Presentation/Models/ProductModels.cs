using MyModularMonolith.Modules.Gyms.Contracts;

namespace MyModularMonolith.Modules.Gyms.Presentation.Models
{
    public record ProductResponse(
        Guid Id,
        string Name,
        string Description,
        decimal BasePrice,
        bool RequiresSchedule,
        bool RequiresInstructor,
        bool HasCapacityLimits,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt)
    {
        public static ProductResponse FromDto(ProductDto dto)
            => new(dto.Id, dto.Name, dto.Description, dto.BasePrice,
                   dto.RequiresSchedule, dto.RequiresInstructor, dto.HasCapacityLimits,
                   dto.IsActive, dto.CreatedAt, dto.UpdatedAt);
    }

    public record ProductsListResponse(
        List<ProductResponse> Products,
        int TotalCount,
        DateTime ResponseTimestamp)
    {
        public static ProductsListResponse FromDtos(List<ProductDto> dtos)
            => new(
                dtos.Select(ProductResponse.FromDto).ToList(),
                dtos.Count,
                DateTime.UtcNow);
    }

    public record CreateProductRequest(
        string Name,
        string Description,
        decimal BasePrice,
        bool RequiresSchedule,
        bool RequiresInstructor,
        bool HasCapacityLimits);
}
