using MyModularMonolith.Modules.Users.Contracts;

namespace MyModularMonolith.Modules.Users.Presentation.Models;

public record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    Guid? HomeGymId,
    string? HomeGymName,
    DateTime RegistrationDate,
    bool IsActive)
{
    public static UserResponse FromDto(UserDto dto)
        => new(dto.Id, dto.Email, dto.FirstName, dto.LastName,
               dto.Role, dto.HomeGymId, dto.HomeGymName,
               dto.RegistrationDate, dto.IsActive);

}

public record AuthenticationResponse(
    UserResponse User,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    DateTime ResponseTimestamp)
{
    public static AuthenticationResponse FromDto(AuthenticationDto dto)
        => new(UserResponse.FromDto(dto.User), dto.AccessToken, dto.RefreshToken,
               dto.ExpiresAt, DateTime.UtcNow);
}

public record RegistrationResponse(
    UserResponse User,
    bool HasTemporaryPassword,
    string Message,
    DateTime ResponseTimestamp)
{
    public static RegistrationResponse FromDto(UserDto dto)
        => new(UserResponse.FromDto(dto), dto.HasTemporaryPassword,
               dto.HasTemporaryPassword ? "User registered with temporary password. Please change it on first login." : "User registered successfully.",
               DateTime.UtcNow);
}

public record PasswordChangeResponse(
    Guid UserId,
    string Message,
    DateTime ChangedAt,
    DateTime ResponseTimestamp)
{
    public static PasswordChangeResponse FromDto(PasswordChangeDto dto)
        => new(dto.UserId, dto.Message, dto.ChangedAt, DateTime.UtcNow);
}

public record RegisterUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string? Password = null,
    string? Role = null,
    Guid? HomeGymId = null,
    string? HomeGymName = null,
    bool GenerateTemporaryPassword = true);

public record ChangePasswordRequest(
    string Email,
    string CurrentPassword,
    string NewPassword);

public record LoginUserRequest(
    string Email,
    string Password);

public record RefreshTokenRequest(
    string RefreshToken);