using System.Text.Json;
using System.Text.Json.Serialization;

namespace YahooFinanceService.Infrastructure;

/// <summary>
/// JSON converter for Unix timestamp (seconds) to DateTime.
/// </summary>
public sealed class UnixTimestampConverter : JsonConverter<DateTime>
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc/>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var timestamp = reader.GetInt64();
            return UnixEpoch.AddSeconds(timestamp);
        }

        throw new JsonException("Expected number for Unix timestamp.");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var timestamp = (long)(value.ToUniversalTime() - UnixEpoch).TotalSeconds;
        writer.WriteNumberValue(timestamp);
    }
}

/// <summary>
/// JSON converter for nullable Unix timestamp to nullable DateTime.
/// </summary>
public sealed class NullableUnixTimestampConverter : JsonConverter<DateTime?>
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc/>
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            var timestamp = reader.GetInt64();
            return UnixEpoch.AddSeconds(timestamp);
        }

        return null;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            var timestamp = (long)(value.Value.ToUniversalTime() - UnixEpoch).TotalSeconds;
            writer.WriteNumberValue(timestamp);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
