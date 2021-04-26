using System.Collections.Generic;
using System.Text.Json;
using Lette.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace Lette.Core.JsonSerialization
{
    public static class JsonSerialization
    {
        public static readonly JsonSerializerOptions Options;

        static JsonSerialization()
        {
            Options = new()
            {
                AllowTrailingCommas = true,
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
                WriteIndented = true,
            };

            Options.Converters.Add(new PointConverter());
            Options.Converters.Add(new Vector2Converter());
            Options.Converters.Add(new RectangleConverter());
            Options.Converters.Add(new EntityConverter());
            Options.Converters.Add(new ValueConverter<Pos, Vector2>());
            Options.Converters.Add(new ValueConverter<KeyMap, Dictionary<Keys, (InputType, float)>>());
            Options.Converters.Add(new ValueConverter<Input, EnumArray<InputType, float>>());
            Options.Converters.Add(new ValueConverter<Id, string>());
            Options.Converters.Add(new Vec2ArrConverter());
            Options.Converters.Add(new TilesConverter());
            Options.Converters.Add(new EnumConverterFactory());
            Options.Converters.Add(new FlagsConverterFactory());
            Options.Converters.Add(new EnumArrayConverterFactory());
            Options.Converters.Add(new EnumDictionaryConverterFactory());
            Options.Converters.Add(new TupleConverterFactory());
        }
    }
}
