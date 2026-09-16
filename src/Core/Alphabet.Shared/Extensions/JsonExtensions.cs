using System.Text.Json;

namespace Alphabet.Shared.Extensions;

public static class JsonExtensions
{
    public static string ToJson<T>(this T value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(value, options);
    }
}
