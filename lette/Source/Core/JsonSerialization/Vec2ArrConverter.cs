using System.Text.Json.Serialization;
using System;
using Microsoft.Xna.Framework;
using System.Text.Json;
using System.Linq;

namespace Lette.Core.JsonSerialization
{
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
}
