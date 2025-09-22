using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Users.Contracts.Commands;

public record RefreshTokenCommand(
string RefreshToken,
string? IpAddress = null) : IRequest<ErrorOr<RefreshTokenResult>>;

public record RefreshTokenResult(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpires);
