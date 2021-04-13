using System.Text.Json.Serialization;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using System.Text.Json;
using Lette.Resources;
using System.Reflection;
using System.Linq;
using Lette.Components;
using Microsoft.Xna.Framework.Input;

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

    public class ValueConverter<T, V> : JsonConverter<T> where T : IValue<V>, new()
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = JsonSerializer.Deserialize<V>(ref reader, options);
            if (value == null)
                throw new Exception();
            return new() { Value = value };
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class Vec2ArrConverter : JsonConverter<Vector2[]>
    {
        public override Vector2[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            JsonSerializer
            .Deserialize<float[]>(ref reader, options)?
            .Select2((x, y) =>  new Vector2(x, y))
            .ToArray();

        public override void Write(Utf8JsonWriter writer, Vector2[] value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class TilesConverter : JsonConverter<Components.Tile[,,]>
    {
        public override Components.Tile[,,]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new Exception();
            reader
                .Advance(out var w)
                .Advance(out var h)
                .Advance(out var d);
            var idx = new Components.Tile[w, h, d];
            for (var x = 0; x < w; x++)
                for (var y = 0; y < h; y++)
                    for (var z = 0; z < d; z++)
                    {
                        reader
                            .Advance(out var entry)
                            .Advance(out var px)
                            .Advance(out var py)
                            .Advance(out var height);

                        idx[x, y, z] = new Components.Tile()
                        {
                            Entry = entry,
                            Idx = new Point(px, py),
                            Height = height
                        };
                    }

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
                throw new Exception();
            return idx;
        }

        public override void Write(Utf8JsonWriter writer, Components.Tile[,,] value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class EntityConverter : JsonConverter<EntityDefinition>
    {
        static readonly Dictionary<string, Type> ComponentTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IReplaceOnEntity)))
            .ToDictionary(value => value.Name.ToCamel());

        public override EntityDefinition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new Exception();
            var def = new EntityDefinition();
            while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
            {
                var name = reader.GetString();
                if (!(name is string))
                    throw new Exception();
                var type = ComponentTypes[name];
                var value = JsonSerializer.Deserialize(ref reader, type, options) as IReplaceOnEntity;
                if (value == null)
                    throw new Exception();
                def.Add(value);
            }
            if (reader.TokenType != JsonTokenType.EndObject)
                throw new Exception();
            return def;

        }

        public override void Write(Utf8JsonWriter writer, EntityDefinition value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
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
            options.Converters.Add(new EntityConverter());
            options.Converters.Add(new ValueConverter<Pos, Vector2>());
            options.Converters.Add(new ValueConverter<KeyMap, Dictionary<Keys, (InputType, float)>>());
            options.Converters.Add(new ValueConverter<Input, EnumArray<InputType, float>>());
            //options.Converters.Add(new)

            return options;
        }
    }
}
