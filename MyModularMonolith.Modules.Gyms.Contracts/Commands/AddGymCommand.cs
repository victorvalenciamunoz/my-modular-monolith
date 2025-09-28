using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Gyms.Contracts.Commands;

public record AddGymCommand(string Name) : IRequest<ErrorOr<GymDto>>;
