using System.Text.Json.Serialization;
using System;
using Microsoft.Xna.Framework;
using System.Text.Json;

namespace Lette.Core
{
    public class PointConverter : JsonConverter<Point>
    {
        public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new Exception();
            int x, y;
            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new Exception();
            x = reader.GetInt32();
            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new Exception();
            y = reader.GetInt32();
            if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
                throw new Exception();
            return new Point(x, y);
        }

        public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
        }
    }

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

    public class RectangleConverter : JsonConverter<Rectangle>
    {
        public override Rectangle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new Exception();
            int x, y, w, h;
            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new Exception();
            x = reader.GetInt32();
            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new Exception();
            y = reader.GetInt32();
            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new Exception();
            w = reader.GetInt32();
            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new Exception();
            h = reader.GetInt32();
            if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
                throw new Exception();
            return new Rectangle(x, y, w, h);
        }
        public override void Write(Utf8JsonWriter writer, Rectangle value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteNumberValue(value.Width);
            writer.WriteNumberValue(value.Height);
            writer.WriteEndArray();
        }
    }

    public class FlagsConverter<T> : JsonConverter<Flags<T>> where T : struct, IConvertible
    {
        public override Flags<T> Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new Exception();
            var flags = Flags<T>.New();
            while (reader.Read() && reader.TokenType == JsonTokenType.String)
            {
                var name = reader.GetString();
                if (name is string && Enum<T>.Map.TryGetValue(name, out var value))
                    flags[value] = true;
                else
                    throw new Exception();
            }
            if (reader.TokenType != JsonTokenType.EndArray)
                throw new Exception();
            return flags;
        }

        public override void Write(Utf8JsonWriter writer, Flags<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var (name, flag) in value.Entries)
                writer.WriteStringValue(name);
            writer.WriteEndArray();

        }
    }

    public static class JsonSerialization
    {
        public static JsonSerializerOptions GetOptions()
        {
            JsonSerializerOptions options = new()
            {
                AllowTrailingCommas = true,
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
                WriteIndented = true,
            };

            options.Converters.Add(new PointConverter());
            options.Converters.Add(new Vector2Converter());
            options.Converters.Add(new RectangleConverter());
            options.Converters.Add(new FlagsConverter<AnimFlag>());

            return options;
        }
    }
}
