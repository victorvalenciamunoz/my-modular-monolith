using Ardalis.GuardClauses;

namespace MyModularMonolith.Shared.Domain.ValueObjects;

public sealed record UserId : ValueObject<string>
{
    private UserId(string value) : base(value) { }

    public static UserId Create(string userId)
    {
        Guard.Against.NullOrWhiteSpace(userId, nameof(userId));
        return new UserId(userId);
    }

    public static UserId CreateNew() => new(Guid.NewGuid().ToString());

    public static implicit operator UserId(string userId) => Create(userId);

    public bool IsValid => !string.IsNullOrWhiteSpace(Value);
}
