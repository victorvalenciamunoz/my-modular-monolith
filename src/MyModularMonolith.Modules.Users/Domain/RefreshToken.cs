using Ardalis.GuardClauses;
using MyModularMonolith.Shared.Domain;

namespace MyModularMonolith.Modules.Users.Domain;

public class RefreshToken : BaseEntity
{
    public string Token { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string CreatedByIp { get; private set; } = string.Empty;

    public ApplicationUser User { get; private set; } = null!;

    private RefreshToken() { } // EF Constructor

    public RefreshToken(string token, Guid userId, DateTime expiresAt, string createdByIp, DateTime createdAt)
    {
        Token = Guard.Against.NullOrEmpty(token, nameof(token));
        UserId = Guard.Against.Default(userId, nameof(userId));
        ExpiresAt = Guard.Against.Default(expiresAt, nameof(expiresAt));
        CreatedByIp = Guard.Against.NullOrEmpty(createdByIp, nameof(createdByIp));
        SetCreated(createdAt);
    }

    public bool IsActive(DateTime currentTime) => !IsRevoked && !IsExpired(currentTime);
    public bool IsExpired(DateTime currentTime) => currentTime >= ExpiresAt;

    public void Revoke(DateTime revokedAt, string? byIp = null, string? replacedByToken = null)
    {
        IsRevoked = true;
        RevokedAt = revokedAt;
        RevokedByIp = byIp;
        ReplacedByToken = replacedByToken;
        SetUpdated(revokedAt);
    }
}
