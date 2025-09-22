using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MyModularMonolith.Modules.Users.Application.Services;
using MyModularMonolith.Modules.Users.Domain;
using MyModularMonolith.Modules.Users.Domain.Events;

namespace MyModularMonolith.Modules.Users.Application.EventHandlers;

internal class UserRegisteredDomainEventHandler : INotificationHandler<UserRegisteredDomainEvent>
{
    private readonly IEmailService _emailService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserRegisteredDomainEventHandler> _logger;

    public UserRegisteredDomainEventHandler(
        IEmailService emailService,
        UserManager<ApplicationUser> userManager,
        ILogger<UserRegisteredDomainEventHandler> logger)
    {
        _emailService = emailService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing UserRegisteredDomainEvent for user {UserId}", notification.UserId);

            var user = await _userManager.FindByIdAsync(notification.UserId.ToString());
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when trying to send welcome email", notification.UserId);
                return;
            }

            string? temporaryPassword = null;
            if (notification.HasTemporaryPassword)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                var tempPasswordClaim = claims.FirstOrDefault(c => c.Type == "TemporaryPassword");
                temporaryPassword = tempPasswordClaim?.Value;

                if (tempPasswordClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, tempPasswordClaim);
                }
            }

            await _emailService.SendWelcomeEmailAsync(
                notification.Email,
                notification.FirstName,
                notification.LastName,
                temporaryPassword,
                cancellationToken);

            _logger.LogInformation("Welcome email sent successfully for user {UserId} at {Email}",
                notification.UserId, notification.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email for user {UserId}", notification.UserId);
        }
    }
}