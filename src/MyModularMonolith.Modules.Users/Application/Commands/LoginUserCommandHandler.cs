using Ardalis.GuardClauses;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MyModularMonolith.Modules.Users.Application.Security;
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
    private readonly ILoginAttemptService _loginAttemptService;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        ILoginAttemptService loginAttemptService,
        ILogger<LoginUserCommandHandler> logger)
    {
        _userManager = Guard.Against.Null(userManager);
        _signInManager = Guard.Against.Null(signInManager);
        _jwtService = Guard.Against.Null(jwtService);
        _refreshTokenRepository = Guard.Against.Null(refreshTokenRepository);
        _unitOfWork = Guard.Against.Null(unitOfWork);
        _dateTimeProvider = Guard.Against.Null(dateTimeProvider);
        _loginAttemptService = Guard.Against.Null(loginAttemptService);
        _logger = logger;
    }

    public async Task<ErrorOr<LoginUserResult>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var currentTime = _dateTimeProvider.UtcNow;

        // 1️⃣ STEP 1: Verificar si la cuenta está bloqueada por demasiados intentos fallidos
        var isLocked = await _loginAttemptService.IsAccountLockedAsync(request.Email, currentTime);
        if (isLocked)
        {
            _logger.LogWarning(
                "Login attempt blocked for {Email} from IP {IpAddress}: Account is temporarily locked",
                request.Email, request.IpAddress);

            return Error.Forbidden(
                "User.AccountLocked",
                "Account temporarily locked due to multiple failed login attempts. Please try again later.");
        }

        // 2️⃣ STEP 2: Buscar usuario por email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning(
                "Login attempt failed for non-existent email {Email} from IP {IpAddress}",
                request.Email, request.IpAddress);

            // Registrar intento fallido (protección contra enumeración de cuentas)
            await _loginAttemptService.RecordFailedAttemptAsync(request.Email, currentTime);

            return Error.NotFound("User.NotFound", "Invalid email or password.");
        }

        // 3️⃣ STEP 3: Verificar si usuario está activo
        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {UserId}", user.Id);
            await _loginAttemptService.RecordFailedAttemptAsync(request.Email, currentTime);
            return Error.Validation("User.Inactive", "User account is inactive.");
        }

        // 4️⃣ STEP 4: Verificar expiración de contraseña temporal
        if (user.IsTemporaryPasswordExpired(currentTime))
        {
            _logger.LogWarning("Login attempt with expired temporary password for user: {UserId}", user.Id);
            await _loginAttemptService.RecordFailedAttemptAsync(request.Email, currentTime);
            return Error.Validation("User.TemporaryPasswordExpired",
                "Temporary password has expired. Please contact administrator.");
        }

        // 5️⃣ STEP 5: Validar contraseña
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Invalid password for user: {UserId} from IP {IpAddress}", user.Id, request.IpAddress);

            // Registrar intento fallido y verificar si la cuenta quedó bloqueada
            var accountLockedNow = await _loginAttemptService.RecordFailedAttemptAsync(request.Email, currentTime);

            if (accountLockedNow)
            {
                return Error.Forbidden(
                    "User.AccountLocked",
                    "Account temporarily locked due to multiple failed login attempts. Please try again later.");
            }

            return Error.Validation("User.InvalidCredentials", "Invalid email or password.");
        }

        // 6️⃣ STEP 6: Login exitoso - Generar tokens
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

        // 7️⃣ STEP 7: Limpiar intentos fallidos previos
        await _loginAttemptService.RecordSuccessfulLoginAsync(request.Email);

        _logger.LogInformation("User {UserId} ({Email}) logged in successfully from IP {IpAddress}", 
            user.Id, request.Email, request.IpAddress);

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