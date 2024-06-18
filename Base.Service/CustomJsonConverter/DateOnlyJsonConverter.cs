﻿using Newtonsoft.Json;

namespace Base.Service.CustomJsonConverter;

// For Serialization to ResponseVM
internal class DateOnlyJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(DateOnly);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (!(value is DateOnly))
        {
            throw new JsonSerializationException("Expected DateOnly object value.");
        }

        var date = (DateOnly?)value;
        writer.WriteValue(date?.ToString("yyyy-MM-dd"));
    }
}