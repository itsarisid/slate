namespace Alphabet.Shared.Extensions;

public static class EnumExtensions
{
    public static string ToDisplayName(this Enum value)
    {
        return value.ToString();
    }
}
