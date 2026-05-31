using System.Text.Json;

namespace Alphabet.Domain.Models;

/// <summary>
/// Provides JSON serialization helpers for the asset management module.
/// </summary>
public static class AssetManagementJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Serializes a value to JSON.
    /// </summary>
    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, Options);
    }

    /// <summary>
    /// Deserializes a JSON string to a value.
    /// </summary>
    public static T? Deserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, Options);
    }

    /// <summary>
    /// Deserializes a JSON string to a list.
    /// </summary>
    public static IReadOnlyList<T> DeserializeList<T>(string? json)
    {
        return Deserialize<IReadOnlyList<T>>(json) ?? Array.Empty<T>();
    }

    /// <summary>
    /// Deserializes a JSON string to a dictionary.
    /// </summary>
    public static IReadOnlyDictionary<string, string> DeserializeDictionary(string? json)
    {
        return Deserialize<IReadOnlyDictionary<string, string>>(json) ?? new Dictionary<string, string>();
    }
}
