using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Users.Contracts.Queries.GetByUserId;

public record GetUserByIdQuery(Guid UserId) : IRequest<ErrorOr<UserDto>>;


