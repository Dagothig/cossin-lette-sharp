using System.Text.Json.Serialization;
using System;
using Microsoft.Xna.Framework;
using System.Text.Json;

namespace Lette.Core.JsonSerialization
{
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
}
