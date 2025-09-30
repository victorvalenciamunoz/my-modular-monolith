using ErrorOr;
using MediatR;
using MyModularMonolith.Shared.Domain.ValueObjects;

namespace MyModularMonolith.Modules.Users.Contracts.Commands;

public record RegisterUserCommand(
        Email Email,
        string FirstName,
        string LastName,
        string Role,
        string? Password = null,
        Guid? HomeGymId = null,
        string? HomeGymName = null,
        bool GenerateTemporaryPassword = true) : IRequest<ErrorOr<UserDto>>;

