using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Users.Contracts.Commands;

public record ChangePasswordCommand(
string Email,
string CurrentPassword,
string NewPassword) : IRequest<ErrorOr<Guid>>;

