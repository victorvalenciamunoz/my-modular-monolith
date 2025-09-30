using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Gyms.Contracts.Queries;

public record GetActiveGymsQuery() : IRequest<ErrorOr<List<GymDto>>>;

