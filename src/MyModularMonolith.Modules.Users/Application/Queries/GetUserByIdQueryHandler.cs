using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyModularMonolith.Modules.Users.Contracts;
using MyModularMonolith.Modules.Users.Contracts.Queries.GetByUserId;
using MyModularMonolith.Modules.Users.Domain;

namespace MyModularMonolith.Modules.Users.Application.Queries;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ErrorOr<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByIdQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ErrorOr<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            return Error.NotFound("User.NotFound", $"UserId {request.UserId} not found");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault(); // Assuming a user has one primary role

        return new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            role ?? string.Empty,
            user.HomeGymId,
            user.HomeGymName,
            user.CreatedAt,
            user.IsActive,
            null, // TemporaryPassword
            user.HasTemporaryPassword);
    }
}
