namespace MyModularMonolith.Modules.Gyms.Contracts;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,    
    decimal BasePrice,
    bool RequiresSchedule,
    bool RequiresInstructor,
    bool HasCapacityLimits,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record GymProductDto(
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
    DateTime? UpdatedAt);
