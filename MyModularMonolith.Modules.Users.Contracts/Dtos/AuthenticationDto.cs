namespace MyModularMonolith.Modules.Users.Contracts;

public record AuthenticationDto(
    UserDto User,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);

public record PasswordChangeDto(
    Guid UserId,
    string Message,
    DateTime ChangedAt);
