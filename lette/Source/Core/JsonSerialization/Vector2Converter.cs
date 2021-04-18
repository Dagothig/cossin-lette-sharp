using System.Text.Json.Serialization;
using System;
using Microsoft.Xna.Framework;
using System.Text.Json;

namespace Lette.Core.JsonSerialization
{
    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new Exception();
            float x, y;
            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new Exception();
            x = (float)reader.GetDouble();
            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new Exception();
            y = (float)reader.GetDouble();
            if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
                throw new Exception();
            return new Vector2(x, y);
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
        }
    }
}
