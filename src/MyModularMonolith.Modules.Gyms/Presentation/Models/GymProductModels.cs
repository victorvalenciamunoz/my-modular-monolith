using MyModularMonolith.Modules.Gyms.Contracts;

namespace MyModularMonolith.Modules.Gyms.Presentation.Models;

public record GymProductResponse(
    Guid Id,
    Guid GymId,
    string GymName,
    Guid ProductId,
    string ProductName,
    string ProductDescription,
    decimal Price,
    decimal? DiscountPercentage,
    decimal FinalPrice,
    bool IsActive,
    string? Schedule,
    int? MinCapacity,
    int? MaxCapacity,
    string? InstructorName,
    string? InstructorEmail,
    string? InstructorPhone,
    string? Notes,
    string? Equipment,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static GymProductResponse FromDto(GymProductDto dto)
        => new(dto.Id, dto.GymId, dto.GymName, dto.ProductId, dto.ProductName,
               dto.ProductDescription, dto.Price, dto.DiscountPercentage,
               dto.FinalPrice, dto.IsActive, dto.Schedule, dto.MinCapacity, dto.MaxCapacity,
               dto.InstructorName, dto.InstructorEmail, dto.InstructorPhone,
               dto.Notes, dto.Equipment, dto.CreatedAt, dto.UpdatedAt);
}

public record GymProductsListResponse(
    List<GymProductResponse> GymProducts,
    int TotalCount,
    DateTime ResponseTimestamp)
{
    public static GymProductsListResponse FromDtos(List<GymProductDto> dtos)
        => new(
            dtos.Select(GymProductResponse.FromDto).ToList(),
            dtos.Count,
            DateTime.UtcNow);
}

public record AddProductToGymRequest(
    Guid ProductId,
    decimal Price,
    decimal? DiscountPercentage = null,
    string? Schedule = null,
    int? MinCapacity = null,
    int? MaxCapacity = null,
    string? InstructorName = null,
    string? InstructorEmail = null,
    string? InstructorPhone = null,
    string? Notes = null,
    string? Equipment = null);