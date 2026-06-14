using System.Text.Json;

namespace Alphabet.Shared.Helpers;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new(JsonSerializerDefaults.Web);

    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, DefaultOptions);
    }

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }
}
