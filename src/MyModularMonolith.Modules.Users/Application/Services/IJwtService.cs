using ErrorOr;
using MyModularMonolith.Modules.Users.Domain;

namespace MyModularMonolith.Modules.Users.Application.Services;

public interface IJwtService
{
    Task<ErrorOr<string>> GenerateAccessTokenAsync(ApplicationUser user);
    string GenerateRefreshToken();
    ErrorOr<Guid> ValidateAccessToken(string token);
}
