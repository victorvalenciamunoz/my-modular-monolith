namespace MyModularMonolith.Modules.Gyms.Contracts;

public record ReservationDto(
    Guid Id,
    string UserId,
    Guid GymProductId,
    string GymName,
    string ProductName,
    DateTime ReservationDateTime,
    string Status,
    decimal Price,
    decimal? DiscountPercentage,
    decimal FinalPrice,
    string? UserNotes,
    string? CancellationReason,
    DateTime? CancelledAt,
    DateTime CreatedAt);