using System.Text.Json;

namespace Alphabet.Domain.Models;

/// <summary>
/// Provides JSON helpers for leave management value snapshots.
/// </summary>
public static class LeaveManagementJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Serializes a value to JSON.
    /// </summary>
    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    /// <summary>
    /// Deserializes a JSON value.
    /// </summary>
    public static T? Deserialize<T>(string? json)
    {
        return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, Options);
    }

    /// <summary>
    /// Deserializes a JSON list.
    /// </summary>
    public static IReadOnlyList<T> DeserializeList<T>(string? json)
    {
        return Deserialize<IReadOnlyList<T>>(json) ?? Array.Empty<T>();
    }

    /// <summary>
    /// Deserializes a JSON dictionary.
    /// </summary>
    public static IReadOnlyDictionary<string, string> DeserializeDictionary(string? json)
    {
        return Deserialize<IReadOnlyDictionary<string, string>>(json) ?? new Dictionary<string, string>();
    }
}
