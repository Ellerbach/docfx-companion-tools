using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocAssembler.Utils;

/// <summary>
/// Serialization utilities.
/// </summary>
public static class SerializationUtil
{
    /// <summary>
    /// Gets the JSON serializer options.
    /// </summary>
    public static JsonSerializerOptions Options => new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(),
        },
    };

    /// <summary>
    /// Serialize object.
    /// </summary>
    public static string Serialize(object value) => JsonSerializer.Serialize(value, Options);

    /// <summary>
    /// Deserialize JSON string.
    /// </summary>
    /// <typeparam name="T">Target type.</typeparam>
    public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options)!;
}
