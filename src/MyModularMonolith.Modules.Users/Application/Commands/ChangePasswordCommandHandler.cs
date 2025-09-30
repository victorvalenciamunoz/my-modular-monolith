using Ardalis.GuardClauses;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyModularMonolith.Modules.Users.Contracts.Commands;
using MyModularMonolith.Modules.Users.Domain;
using MyModularMonolith.Shared.Application;

namespace MyModularMonolith.Modules.Users.Application.Commands;

internal class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ErrorOr<Guid>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ChangePasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _userManager = Guard.Against.Null(userManager);
        _unitOfWork = Guard.Against.Null(unitOfWork);
        _dateTimeProvider = Guard.Against.Null(dateTimeProvider);
    }

    public async Task<ErrorOr<Guid>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {        
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Error.NotFound("User.NotFound", "User not found.");
        }
                
        if (!user.IsActive)
        {
            return Error.Validation("User.Inactive", "User account is inactive.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!passwordValid)
        {
            return Error.Validation("User.InvalidCurrentPassword", "Current password is incorrect.");
        }

        var samePassword = await _userManager.CheckPasswordAsync(user, request.NewPassword);
        if (samePassword)
        {
            return Error.Validation("User.SamePassword", "New password must be different from current password.");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => Error.Validation($"Password.{e.Code}", e.Description)).ToList();
            return ErrorOr<Guid>.From(errors);
        }
                
        var currentTime = _dateTimeProvider.UtcNow;
        user.PasswordChanged(currentTime);
                
        await _userManager.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
