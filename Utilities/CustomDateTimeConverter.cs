using Newtonsoft.Json;
using System;

public class CustomDateTimeConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is DateTime dateTime)
        {
            writer.WriteValue(dateTime.ToString("yyyy-MM-dd")); // Format as desired
        }
        else
        {
            writer.WriteNull();
        }
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonToken.Date && reader.Value is DateTime dateTime)
        {
            return dateTime;
        }

        var dateString = reader.Value?.ToString();
        if (DateTime.TryParse(dateString, out var dateTimeParsed))
        {
            return dateTimeParsed;
        }

        throw new JsonSerializationException($"Cannot parse date string: {dateString}");
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
    }
}
