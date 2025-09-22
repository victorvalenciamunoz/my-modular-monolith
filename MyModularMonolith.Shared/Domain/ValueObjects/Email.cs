using Ardalis.GuardClauses;
using System.Text.RegularExpressions;

namespace MyModularMonolith.Shared.Domain.ValueObjects;

public sealed record Email : ValueObject<string>
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled);

    private Email(string value) : base(value) { }

    public static Email Create(string email)
    {
        Guard.Against.NullOrWhiteSpace(email, nameof(email));

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        return new Email(email.ToLowerInvariant());
    }

    public static implicit operator Email(string email) => Create(email);

    public string Domain => Value.Split('@')[1];
    public string LocalPart => Value.Split('@')[0];
}
