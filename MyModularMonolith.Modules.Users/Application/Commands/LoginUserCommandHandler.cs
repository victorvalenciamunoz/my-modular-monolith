using Ardalis.GuardClauses;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MyModularMonolith.Modules.Users.Application.Services;
using MyModularMonolith.Modules.Users.Contracts.Commands;
using MyModularMonolith.Modules.Users.Domain;
using MyModularMonolith.Modules.Users.Domain.Specifications;
using MyModularMonolith.Shared.Application;

namespace MyModularMonolith.Modules.Users.Application.Commands;

internal class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, ErrorOr<LoginUserResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        ILogger<LoginUserCommandHandler> logger)
    {
        _userManager = Guard.Against.Null(userManager);
        _signInManager = Guard.Against.Null(signInManager);
        _jwtService = Guard.Against.Null(jwtService);
        _refreshTokenRepository = Guard.Against.Null(refreshTokenRepository);
        _unitOfWork = Guard.Against.Null(unitOfWork);
        _dateTimeProvider = Guard.Against.Null(dateTimeProvider);
        _logger = logger;
    }

    public async Task<ErrorOr<LoginUserResult>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login attempt failed from IP: {IpAddress} at {Timestamp}",
                                request.IpAddress, _dateTimeProvider.UtcNow); ;
            return Error.NotFound("User.NotFound", "Invalid email or password.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {UserId}", user.Id);
            return Error.Validation("User.Inactive", "User account is inactive.");
        }

        var currentTime = _dateTimeProvider.UtcNow;
        if (user.IsTemporaryPasswordExpired(currentTime))
        {
            _logger.LogWarning("Login attempt with expired temporary password for user: {UserId}", user.Id);
            return Error.Validation("User.TemporaryPasswordExpired",
                "Temporary password has expired. Please contact administrator.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Invalid login attempt for user: {UserId}", user.Id);
            return Error.Validation("User.InvalidCredentials", "Invalid email or password.");
        }

        var accessTokenResult = await _jwtService.GenerateAccessTokenAsync(user);
        if (accessTokenResult.IsError)
        {
            return accessTokenResult.Errors;
        }

        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpires = currentTime.AddDays(7); // 7 days expiration

        var revokeSpec = new RefreshTokenSpecs.NotRevokedByUserId(user.Id);
        await _refreshTokenRepository.RevokeAllBySpecificationAsync(revokeSpec, cancellationToken);

        var refreshTokenEntity = new RefreshToken(
            refreshToken,
            user.Id,
            refreshTokenExpires,
            request.IpAddress ?? "unknown",
            currentTime
        );

        // Save refresh token
        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);
        return new LoginUserResult(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            accessTokenResult.Value,
            refreshToken,
            refreshTokenExpires
        );

    }
}