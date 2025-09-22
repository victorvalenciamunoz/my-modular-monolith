using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Users.Contracts.Commands;

public record LoginUserCommand(
    string Email,
    string Password,
    string? IpAddress = null) : IRequest<ErrorOr<LoginUserResult>>;

public record LoginUserResult(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpires,
    bool MustChangePassword = false,
    bool HasTemporaryPassword = false);
