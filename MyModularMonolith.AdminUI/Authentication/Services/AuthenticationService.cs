using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyModularMonolith.AdminUI.Authentication.Models;

namespace MyModularMonolith.AdminUI.Authentication.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly BackendHttpClient _backendHttpClient;
    private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        BackendHttpClient backendHttpClient,
        CustomAuthenticationStateProvider authenticationStateProvider,
        ILogger<AuthenticationService> logger)
    {
        _backendHttpClient = backendHttpClient;
        _authenticationStateProvider = authenticationStateProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<Success>> LoginAsync(LoginModel loginModel)
    {
        try
        {
            _logger.LogInformation("Login attempt for user {Email}", loginModel.Email);

            var response = await _backendHttpClient.HttpClient.PostAsJsonAsync("api/auth/login", loginModel);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
                if (authResponse?.User is not null)
                {
                    if(authResponse.User.Role is null)
                    {
                        _logger.LogWarning("Login failed for user {Email}: Role is null", loginModel.Email);
                        return Error.Unauthorized("Login.InvalidCredentials", "Invalid credentials");
                    }
                    if (!authResponse.User.Role.Equals(UserRoles.SuperAdmin))
                    {
                        _logger.LogWarning(
                            "Login failed for user {Email}: Insufficient permissions. User role: {Role}, Required: {RequiredRole}",
                            loginModel.Email, authResponse.User.Role, UserRoles.SuperAdmin);
                        return Error.Forbidden("Login.InvalidRole", "You don't have permissions");
                    }

                    // Validar que se recibieron tokens
                    if (string.IsNullOrEmpty(authResponse.AccessToken) || string.IsNullOrEmpty(authResponse.RefreshToken))
                    {
                        _logger.LogError(
                            "Login failed for user {Email}: Tokens not received from backend. AccessToken present: {HasAccessToken}, RefreshToken present: {HasRefreshToken}",
                            loginModel.Email, !string.IsNullOrEmpty(authResponse.AccessToken), !string.IsNullOrEmpty(authResponse.RefreshToken));
                        return Error.Unexpected("Login.MissingTokens", "Authentication tokens were not received.");
                    }

                    var userSession = new UserSession(
                        $"{authResponse.User.FirstName} {authResponse.User.LastName}",
                        authResponse.User.Email!,
                        authResponse.User.Role!);

                    // Calcular fecha de expiración (convertir Unix timestamp a DateTime)
                    var expiresAt = authResponse.RefreshTokenExpires > 0 
                        ? DateTimeOffset.FromUnixTimeSeconds(authResponse.RefreshTokenExpires).UtcDateTime
                        : DateTime.UtcNow.AddHours(1); // Fallback: 1 hora

                    if (authResponse.RefreshTokenExpires <= 0)
                    {
                        _logger.LogWarning(
                            "Login for user {Email}: Invalid RefreshTokenExpires value ({Value}), using default 1 hour expiration",
                            loginModel.Email, authResponse.RefreshTokenExpires);
                    }

                    // Actualizar estado con tokens
                    await _authenticationStateProvider.UpdateAuthenticationStateAsync(
                        userSession,
                        authResponse.AccessToken,
                        authResponse.RefreshToken,
                        expiresAt);

                    _logger.LogInformation(
                        "Login successful for user {Email} with role {Role}, session expires at {ExpiresAt:u}",
                        loginModel.Email, authResponse.User.Role, expiresAt);

                    return Result.Success;
                }

                _logger.LogWarning("Login failed for user {Email}: User data not present in response", loginModel.Email);
                return Error.Unexpected("Login.InvalidCredentials", "Invalid credentials.");
            }

            var statusCode = (int)response.StatusCode;
            _logger.LogWarning(
                "Login failed for user {Email}: Backend returned {StatusCode}",
                loginModel.Email, statusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            return Error.Failure(
                code: problemDetails?.Title ?? "Login.Failed",
                description: problemDetails?.Detail ?? "An unknown error occurred during login.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, 
                "Network error during login for user {Email}: {Message}",
                loginModel.Email, ex.Message);
            return Error.Failure("Login.NetworkError", "Unable to connect to authentication service");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for user {Email}", loginModel.Email);
            return Error.Failure("Login.UnexpectedError", "An unexpected error occurred during login");
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            _logger.LogInformation("Logout initiated");
            await _authenticationStateProvider.UpdateAuthenticationStateAsync(null);
            _logger.LogInformation("Logout completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            throw;
        }
    }
}
