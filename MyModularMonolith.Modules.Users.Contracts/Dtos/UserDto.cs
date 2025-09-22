namespace MyModularMonolith.Modules.Users.Contracts;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    Guid? HomeGymId,
    string? HomeGymName,
    DateTime RegistrationDate,
    bool IsActive,
    string? TemporaryPassword = null,
    bool HasTemporaryPassword = false);