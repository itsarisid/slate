namespace Alphabet.Shared.Extensions;

public static class DateTimeExtensions
{
    public static DateOnly ToDateOnly(this DateTimeOffset value)
    {
        return DateOnly.FromDateTime(value.UtcDateTime);
    }
}
