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
                var value = (T?)JsonSerializer.Deserialize(ref reader, typeof(T), options);
                if (!value.HasValue)
                    throw new Exception();
                else
                    flags[value.Value] = true;
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

    public class FlagsConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType &&
            typeToConvert.GetGenericTypeDefinition() == typeof(Flags<>);

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter?)Activator.CreateInstance(
                typeof(FlagsConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()));
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
            .Deserialize<float[]>(ref reader, options)
            ?.Select2((x, y) =>  new Vector2(x, y))
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
            for (var z = 0; z < d; z++)
                for (var y = 0; y < h; y++)
                    for (var x = 0; x < w; x++)
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

    public abstract class MapConverter<T> : JsonConverter<T>
    {
        public abstract T Init();
        public abstract void ReadValue(ref Utf8JsonReader reader, ref T obj, string name, JsonSerializerOptions options);

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new Exception();
            var obj = Init();
            while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
            {
                var name = reader.GetString();
                if (!(name is string))
                    throw new Exception();
                ReadValue(ref reader, ref obj, name, options);
            }
            if (reader.TokenType != JsonTokenType.EndObject)
                throw new Exception();
            return obj;
        }
    }

    public class EntityConverter : MapConverter<EntityDefinition>
    {
        static readonly Dictionary<string, Type> ComponentTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IReplaceOnEntity)))
            .ToDictionary(value => value.Name.ToCamel());

        public override EntityDefinition Init() => new EntityDefinition();

        public override void ReadValue(
            ref Utf8JsonReader reader,
            ref EntityDefinition def,
            string name,
            JsonSerializerOptions options)
        {
            var type = ComponentTypes[name];
            var value = JsonSerializer.Deserialize(ref reader, type, options) as IReplaceOnEntity;
            if (value == null)
                throw new Exception();
            def.Add(value);
        }

        public override void Write(Utf8JsonWriter writer, EntityDefinition value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumConverter<T> : JsonConverter<T> where T : struct, IConvertible
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new Exception();
            var str = reader.GetString()?.ToPascal() ?? string.Empty;
            if (!Enum<T>.Map.TryGetValue(str, out var r))
                throw new Exception($"Unknown value { str } for enum { typeof(T).ToString() }");
            return r;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter?)Activator.CreateInstance(
                typeof(EnumConverter<>).MakeGenericType(typeToConvert));
    }

    public class EnumArrayConverter<K, V> : MapConverter<EnumArray<K, V>>
    where K : struct, IConvertible
    where V : new()
    {
        public override EnumArray<K, V> Init() => EnumArray<K, V>.New();

        public override void ReadValue(
            ref Utf8JsonReader reader,
            ref EnumArray<K, V> arr,
            string name,
            JsonSerializerOptions options)
        {
            if (!Enum<K>.Map.TryGetValue(name.ToPascal(), out var key))
                throw new Exception($"Unknown value { name } for enum { typeof(K).ToString() }");
            var value = JsonSerializer.Deserialize<V>(ref reader, options);
            if (value == null)
                throw new Exception();
            arr[key] = value;
        }

        public override void Write(Utf8JsonWriter writer, EnumArray<K, V> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumArrayConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType &&
            typeToConvert.GetGenericTypeDefinition() == typeof(EnumArray<,>);

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter?)Activator.CreateInstance(
                typeof(EnumArrayConverter<,>).MakeGenericType(typeToConvert.GetGenericArguments()));
    }

    public class EnumDictionaryConverter<K, V> : MapConverter<Dictionary<K, V>>
    where K : struct, IConvertible
    where V : new()
    {
        public override Dictionary<K, V> Init() => new();

        public override void ReadValue(
            ref Utf8JsonReader reader,
            ref Dictionary<K, V> arr,
            string name,
            JsonSerializerOptions options)
        {
            if (!Enum<K>.Map.TryGetValue(name.ToPascal(), out var key))
                throw new Exception($"Unknown value { name } for enum { typeof(K).ToString() }");
            var value = JsonSerializer.Deserialize<V>(ref reader, options);
            if (value == null)
                throw new Exception();
            arr[key] = value;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<K, V> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumDictionaryConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType &&
            typeToConvert.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
            typeToConvert.GetGenericArguments()[0].IsEnum;

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter?)Activator.CreateInstance(
                typeof(EnumDictionaryConverter<,>).MakeGenericType(typeToConvert.GetGenericArguments()));
    }

    public class TupleConverter<A, B> : JsonConverter<(A, B)> where A : notnull where B : notnull
    {
        public override (A, B) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new Exception();
            if (!reader.Read() || reader.TokenType == JsonTokenType.EndArray)
                throw new Exception();
            var a = JsonSerializer.Deserialize<A>(ref reader, options);
            if (!reader.Read() || reader.TokenType == JsonTokenType.EndArray)
                throw new Exception();
            var b = JsonSerializer.Deserialize<B>(ref reader, options);
            if (a == null || b == null || !reader.Read() || reader.TokenType != JsonTokenType.EndArray)
                throw new Exception();
            return (a, b);
        }

        public override void Write(Utf8JsonWriter writer, (A, B) value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class TupleConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType &&
            typeToConvert.GetGenericTypeDefinition() == typeof(ValueTuple<,>);

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter?)Activator.CreateInstance(
                typeof(TupleConverter<,>).MakeGenericType(typeToConvert.GetGenericArguments()));
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
            options.Converters.Add(new EntityConverter());
            options.Converters.Add(new ValueConverter<Pos, Vector2>());
            options.Converters.Add(new ValueConverter<KeyMap, Dictionary<Keys, (InputType, float)>>());
            options.Converters.Add(new ValueConverter<Input, EnumArray<InputType, float>>());
            options.Converters.Add(new ValueConverter<Id, string>());
            options.Converters.Add(new Vec2ArrConverter());
            options.Converters.Add(new TilesConverter());
            options.Converters.Add(new EnumConverterFactory());
            options.Converters.Add(new FlagsConverterFactory());
            options.Converters.Add(new EnumArrayConverterFactory());
            options.Converters.Add(new EnumDictionaryConverterFactory());
            options.Converters.Add(new TupleConverterFactory());

            return options;
        }
    }
}
