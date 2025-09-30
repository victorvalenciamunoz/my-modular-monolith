using Ardalis.GuardClauses;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyModularMonolith.Modules.Users.Application.Services;
using MyModularMonolith.Modules.Users.Contracts.Commands;
using MyModularMonolith.Modules.Users.Domain;
using MyModularMonolith.Modules.Users.Domain.Specifications;
using MyModularMonolith.Shared.Application;

namespace MyModularMonolith.Modules.Users.Application.Commands;

internal class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ErrorOr<RefreshTokenResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _userManager = Guard.Against.Null(userManager);
        _jwtService = Guard.Against.Null(jwtService);
        _refreshTokenRepository = Guard.Against.Null(refreshTokenRepository);
        _unitOfWork = Guard.Against.Null(unitOfWork);
        _dateTimeProvider = Guard.Against.Null(dateTimeProvider);
    }

    public async Task<ErrorOr<RefreshTokenResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var currentTime = _dateTimeProvider.UtcNow;

        var tokenSpec = new RefreshTokenSpecs.ActiveByTokenWithUser(request.RefreshToken, currentTime);
        var refreshToken = await _refreshTokenRepository.FirstOrDefaultAsync(tokenSpec, cancellationToken);

        if (refreshToken == null)
        {
            return Error.NotFound("RefreshToken.NotFound", "Refresh token not found or invalid.");
        }

        var user = refreshToken.User;
        if (!user.IsActive)
        {
            return Error.NotFound("User.NotFound", "User is inactive.");
        }

        var accessTokenResult = await _jwtService.GenerateAccessTokenAsync(user);
        if (accessTokenResult.IsError)
        {
            return accessTokenResult.Errors;
        }

        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newRefreshTokenExpires = currentTime.AddDays(7);

        refreshToken.Revoke(currentTime, request.IpAddress, newRefreshToken);

        var newRefreshTokenEntity = new RefreshToken(
            newRefreshToken,
            user.Id,
            newRefreshTokenExpires,
            request.IpAddress ?? "unknown",
            currentTime
        );

        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResult(
            accessTokenResult.Value,
            newRefreshToken,
            newRefreshTokenExpires
        );
    }
}