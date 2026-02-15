using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using MyModularMonolith.AdminUI.Authentication.Models;

namespace MyModularMonolith.AdminUI.Authentication.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly BackendHttpClient _backendHttpClient;
    private readonly CustomAuthenticationStateProvider _authenticationStateProvider;

    public AuthenticationService(
        BackendHttpClient backendHttpClient,
        CustomAuthenticationStateProvider authenticationStateProvider)
    {
        _backendHttpClient = backendHttpClient;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<ErrorOr<Success>> LoginAsync(LoginModel loginModel)
    {
        var response = await _backendHttpClient.HttpClient.PostAsJsonAsync("api/auth/login", loginModel);

        if (response.IsSuccessStatusCode)
        {
            var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            if (authResponse?.User is not null)
            {
                if(authResponse.User.Role is null)
                {
                    return Error.Unauthorized("Login.InvalidCredentials", "Invalid credentials");
                }
                if (!authResponse.User.Role.Equals(UserRoles.SuperAdmin))
                {
                    return Error.Forbidden("Login.InvalidRole", "You don't have permissions");
                }

                // Validar que se recibieron tokens
                if (string.IsNullOrEmpty(authResponse.AccessToken) || string.IsNullOrEmpty(authResponse.RefreshToken))
                {
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

                // Actualizar estado con tokens
                await _authenticationStateProvider.UpdateAuthenticationStateAsync(
                    userSession,
                    authResponse.AccessToken,
                    authResponse.RefreshToken,
                    expiresAt);

                return Result.Success;
            }

            return Error.Unexpected("Login.InvalidCredentials", "Invalid credentials.");
        }

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        return Error.Failure(
            code: problemDetails?.Title ?? "Login.Failed",
            description: problemDetails?.Detail ?? "An unknown error occurred during login.");
    }

    public async Task LogoutAsync()
    {
        await _authenticationStateProvider.UpdateAuthenticationStateAsync(null);
    }
}
