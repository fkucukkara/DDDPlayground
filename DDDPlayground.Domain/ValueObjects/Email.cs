using System.Text.RegularExpressions;

namespace DDDPlayground.Domain.ValueObjects;

/// <summary>
/// Value Object for email addresses with validation.
/// Ensures that email addresses are valid before being used in the domain.
/// </summary>
public sealed record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));

    public string Value { get; init; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email cannot be empty", nameof(value));
        }

        if (!EmailRegex.IsMatch(value))
        {
            throw new ArgumentException($"Invalid email format: {value}", nameof(value));
        }

        Value = value.ToLowerInvariant();
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
