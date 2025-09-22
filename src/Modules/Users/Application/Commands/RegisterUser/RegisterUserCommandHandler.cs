using Microsoft.AspNetCore.Identity;
using ErrorOr;
using Ardalis.GuardClauses;
using MyModularMonolith.Modules.Users.Domain;
using MyModularMonolith.Shared.Application;

namespace MyModularMonolith.Modules.Users.Application.Commands.RegisterUser;

internal class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ErrorOr<RegisterUserResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RegisterUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _userManager = Guard.Against.Null(userManager);
        _roleManager = Guard.Against.Null(roleManager);
        _unitOfWork = Guard.Against.Null(unitOfWork);
        _dateTimeProvider = Guard.Against.Null(dateTimeProvider);
    }

    public async Task<ErrorOr<RegisterUserResult>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Validate role
        if (!UserRoles.IsValidRole(request.Role))
        {
            return Error.Validation("User.InvalidRole", $"Role '{request.Role}' is not valid.");
        }

        // Check if role exists
        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            return Error.NotFound("Role.NotFound", $"Role '{request.Role}' does not exist.");
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Error.Conflict("User.EmailAlreadyExists", "A user with this email already exists.");
        }

        // Create user
        var currentTime = _dateTimeProvider.UtcNow;
        var user = new ApplicationUser(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Email, // Using email as username
            currentTime
        );

        // Create user with password
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => Error.Validation($"User.{e.Code}", e.Description)).ToArray();
            return ErrorOr<RegisterUserResult>.From(errors);
        }

        // Assign role
        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
        {
            // If role assignment fails, delete the created user
            await _userManager.DeleteAsync(user);
            var errors = roleResult.Errors.Select(e => Error.Validation($"Role.{e.Code}", e.Description)).ToArray();
            return ErrorOr<RegisterUserResult>.From(errors);
        }

        // Save domain events
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterUserResult(user.Id, user.Email!, user.FirstName, user.LastName);
    }
}

public interface IRequestHandler<TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}