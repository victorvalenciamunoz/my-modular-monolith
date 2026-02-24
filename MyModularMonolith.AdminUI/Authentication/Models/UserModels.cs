using System.ComponentModel.DataAnnotations;

namespace MyModularMonolith.AdminUI.Authentication.Models;

public class LoginModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthenticationResponse
{
    public UserResponse? User { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public long RefreshTokenExpires { get; set; }
}

public class UserResponse
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
}
