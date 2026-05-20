using System.Text.Json;

namespace Alphabet.Domain.Models;
/// <summary>
/// Productivity json.
/// </summary>

public static class ProductivityJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    /// <summary>
    /// Serialize.
    /// </summary>

    public static string Serialize<T>(IReadOnlyCollection<T>? items)
    {
        return JsonSerializer.Serialize(items ?? Array.Empty<T>(), Options);
    }
    /// <summary>
    /// Serialize.
    /// </summary>

    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, Options);
    }
    /// <summary>
    /// Deserialize list.
    /// </summary>

    public static IReadOnlyList<T> DeserializeList<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<T>();
        }

        return JsonSerializer.Deserialize<IReadOnlyList<T>>(json, Options) ?? Array.Empty<T>();
    }
    /// <summary>
    /// Deserialize.
    /// </summary>

    public static T? Deserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, Options);
    }
}
