namespace GameOfDrones.Domain;

internal static class Names
{
    public const int MaxLength = 30;

    public static string Normalize(string? value, string subject)
    {
        var name = value?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            throw new DomainException($"{subject} name is required.");
        }

        if (name.Length > MaxLength)
        {
            throw new DomainException($"{subject} name cannot exceed {MaxLength} characters.");
        }

        return name;
    }

    public static string ToKey(string name) => name.ToUpperInvariant();
}
