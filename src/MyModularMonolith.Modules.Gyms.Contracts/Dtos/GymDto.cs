namespace MyModularMonolith.Modules.Gyms.Contracts;

public record GymDto(
    Guid Id,
    string Name,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
