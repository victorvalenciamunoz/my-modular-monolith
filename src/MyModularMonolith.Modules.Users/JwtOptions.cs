namespace MyModularMonolith.Modules.Users;

public class JwtOptions
{
    public const string SectionName = "JWT";

    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; init; } = 15;
}
