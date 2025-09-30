using Ardalis.GuardClauses;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;
using MyModularMonolith.Modules.Users.Contracts;
using MyModularMonolith.Modules.Users.Contracts.Commands;
using MyModularMonolith.Modules.Users.Domain;
using MyModularMonolith.Shared.Application;
using System.Security.Claims;
using System.Security.Cryptography;

namespace MyModularMonolith.Modules.Users.Application.Commands;

internal class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ErrorOr<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMediator _mediator;

    public RegisterUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IMediator mediator)
    {
        _userManager = Guard.Against.Null(userManager);
        _roleManager = Guard.Against.Null(roleManager);
        _unitOfWork = Guard.Against.Null(unitOfWork);
        _dateTimeProvider = Guard.Against.Null(dateTimeProvider);
        _mediator = Guard.Against.Null(mediator);
    }

    public async Task<ErrorOr<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (!UserRoles.IsValidRole(request.Role))
        {
            return Error.Validation("User.InvalidRole", $"Role '{request.Role}' is not valid.");
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            return Error.NotFound("Role.NotFound", $"Role '{request.Role}' does not exist.");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email.Value);
        if (existingUser != null)
        {
            return Error.Conflict("User.EmailAlreadyExists", "A user with this email already exists.");
        }

        if (request.Role == UserRoles.User && request.HomeGymId.HasValue)
        {
            var gymQuery = new GetGymByIdQuery(request.HomeGymId.Value);
            var gymResult = await _mediator.Send(gymQuery, cancellationToken);

            if (gymResult.IsError)
            {
                return Error.NotFound("Gym.NotFound", "The specified gym does not exist.");
            }

            if (!gymResult.Value.IsActive)
            {
                return Error.Validation("Gym.Inactive", "The specified gym is not active.");
            }

            request = request with { HomeGymName = gymResult.Value.Name };
        }
        else if (request.Role == UserRoles.User)
        {
            return Error.Validation("User.HomeGymRequired", "Home gym is required for gym members.");
        }

        string passwordToUse;
        string? temporaryPassword = null;
        bool hasTemporaryPassword = false;

        if (string.IsNullOrEmpty(request.Password) && request.GenerateTemporaryPassword)
        {
            temporaryPassword = GenerateTemporaryPassword();
            passwordToUse = temporaryPassword;
            hasTemporaryPassword = true;
        }
        else if (!string.IsNullOrEmpty(request.Password))
        {
            passwordToUse = request.Password;
            hasTemporaryPassword = false;
        }
        else
        {
            return Error.Validation("User.PasswordRequired", "Password is required when not generating temporary password.");
        }

        var currentTime = _dateTimeProvider.UtcNow;
        var user = new ApplicationUser(
            request.Email.Value,
            request.FirstName,
            request.LastName,
            request.Email.Value, // Using email as username
            currentTime,
            request.HomeGymId,
            request.HomeGymName,
            hasTemporaryPassword);

        var result = await _userManager.CreateAsync(user, passwordToUse);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => Error.Validation($"User.{e.Code}", e.Description)).ToList();
            return ErrorOr<UserDto>.From(errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            var errors = roleResult.Errors.Select(e => Error.Validation($"Role.{e.Code}", e.Description)).ToList();
            return ErrorOr<UserDto>.From(errors);
        }

        if (hasTemporaryPassword && !string.IsNullOrEmpty(temporaryPassword))
        {
            await _userManager.AddClaimAsync(user, new Claim("TemporaryPassword", temporaryPassword));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var domainEvents = user.DomainEvents.ToList();
        user.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        return new UserDto(
            user.Id,
            request.Email.Value,
            user.FirstName,
            user.LastName,
            request.Role,
            request.HomeGymId,
            request.HomeGymName,
            _dateTimeProvider.UtcNow,
            user.IsActive,
            temporaryPassword,
            hasTemporaryPassword);
    }

    private static string GenerateTemporaryPassword()
    {
        const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lowercase = "abcdefghijkmnpqrstuvwxyz";
        const string digits = "23456789";

        const int length = 8;

        using var rng = RandomNumberGenerator.Create();
        var password = new char[length];

        password[0] = GetRandomChar(uppercase, rng);
        password[1] = GetRandomChar(lowercase, rng);
        password[2] = GetRandomChar(digits, rng);

        string allChars = uppercase + lowercase + digits;
        for (int i = 3; i < length; i++)
        {
            password[i] = GetRandomChar(allChars, rng);
        }

        ShuffleArray(password, rng);

        return new string(password);
    }

    private static char GetRandomChar(string chars, RandomNumberGenerator rng)
    {
        var bytes = new byte[1];
        rng.GetBytes(bytes);
        return chars[bytes[0] % chars.Length];
    }

    private static void ShuffleArray(char[] array, RandomNumberGenerator rng)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            var bytes = new byte[1];
            rng.GetBytes(bytes);
            int j = bytes[0] % (i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}