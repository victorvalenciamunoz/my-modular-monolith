using Ardalis.GuardClauses;

namespace MyModularMonolith.Shared.Domain.ValueObjects;

public sealed record PersonName : ValueObject<string>
{
    private PersonName(string value) : base(value) { }

    public static PersonName Create(string name)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.LengthOutOfRange(name, 2, 100, nameof(name));
                
        var cleanedName = name.Trim();
        return new PersonName(cleanedName);
    }

    public static implicit operator PersonName(string name) => Create(name);

    public string FirstLetter => Value[0].ToString().ToUpperInvariant();
    public string Initials => string.Join("", Value.Split(' ').Select(n => n[0]).Take(3));
}
