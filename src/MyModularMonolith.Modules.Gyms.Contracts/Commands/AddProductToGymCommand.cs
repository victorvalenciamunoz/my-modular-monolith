using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Gyms.Contracts.Commands;

public record AddProductToGymCommand(
    Guid GymId,
    Guid ProductId,
    decimal Price,
    decimal? DiscountPercentage,
    string? Schedule,
    int? MinCapacity,
    int? MaxCapacity,
    string? InstructorName,
    string? InstructorEmail,
    string? InstructorPhone,
    string? Notes,
    string? Equipment) : IRequest<ErrorOr<GymProductDto>>;
