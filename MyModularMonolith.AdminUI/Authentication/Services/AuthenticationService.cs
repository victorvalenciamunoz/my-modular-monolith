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
                var userSession = new UserSession(
                    $"{authResponse.User.FirstName} {authResponse.User.LastName}",
                    authResponse.User.Email!,
                    authResponse.User.Role!);
                await _authenticationStateProvider.UpdateAuthenticationState(userSession);
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
        await _authenticationStateProvider.UpdateAuthenticationState(null);
    }
}
