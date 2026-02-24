using ErrorOr;
using MyModularMonolith.AdminUI.Authentication.Models;

namespace MyModularMonolith.AdminUI.Authentication.Services;

public interface IAuthenticationService
{
    Task<ErrorOr<Success>> LoginAsync(LoginModel loginModel);
    Task LogoutAsync();
}