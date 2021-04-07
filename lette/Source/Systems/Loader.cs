using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using Leopotam.Ecs;
using Lette.Components;
using Lette.Resources;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Linq;
using Tile = Lette.Resources.Tile;
using Lette.Core;
using System.Text.Json.Serialization;
using System;

namespace Lette.Systems
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
                if (Enum<T>.Map.TryGetValue(name, out var value))
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

    public class Loader : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        JsonSerializerOptions options = null;
        FileSystemWatcher watcher = null;

        Game game = null;
        EcsFilter<Sprite> sprites = null;
        EcsFilter<Tiles> tiles = null;

        Dictionary<string, Task<Sheet>> sheets = new();
        Dictionary<string, Task<Tileset>> tilesets = new();

        public void Init()
        {
            options = new JsonSerializerOptions()
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

            // TODO This should only be in debug
            FileSystemWatcher watcher = new()
            {
                Path = "Content",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            watcher.Created += OnChanged;
            watcher.Changed += OnChanged;
        }

        public void Destroy()
        {
            watcher.Dispose();
        }

        public void OnChanged(object sender, FileSystemEventArgs e)
        {
            e.Name.Split("/", 4).Take(out var folder, out var type, out var resource);

            if (folder == "Content" && type == "img")
            {
                if (sheets.TryGetValue(resource, out var sheet))
                {
                    sheets[resource] = LoadSheet(resource);
                }
                else if (tilesets.TryGetValue(resource, out var tileset))
                {
                    tilesets[resource] = LoadTileset(resource);
                }
            }
        }

        public async Task<Sheet> LoadSheet(string src)
        {
            using (var file = File.OpenRead($"Content/img/{ src }/sheet.json"))
            {
                var sheet = await JsonSerializer.DeserializeAsync<Sheet>(file, options);
                sheet.Src = src;
                await Task.WhenAll(sheet.Entries.Select(entry => Task.Run(() =>
                {
                    entry.Texture = Texture2D.FromFile(game.GraphicsDevice, $"Content/img/{ src }/{ entry.Src }");
                    entry.FrameTime = 1000f / entry.FPS;
                    entry.TilesCount = (int)(entry.Texture.Width / entry.Size.X);
                    var ptSize = entry.Size.ToPoint();
                    entry.Strips = entry.StripsVariants
                        .SelectMany((variants, j) => variants
                            .Select(variant => new Strip
                            {
                                Flags = variant.Item1,
                                Tiles = Enumerable
                                    .Range(0, entry.TilesCount)
                                    .Select(i => new Tile
                                    {
                                        Quad = new Rectangle(new Point(i, j) * ptSize, ptSize),
                                        Scale = variant.Item2
                                    })
                                    .ToArray()
                            }))
                        .ToArray();
                })).ToList());
                return sheet;
            }
        }

        public async Task<Tileset> LoadTileset(string src)
        {
            using (var file = File.OpenRead($"Content/img/{ src }/tileset.json"))
            {
                var tileset = await JsonSerializer.DeserializeAsync<Tileset>(file, options);
                tileset.Src = src;
                await Task.WhenAll(tileset.Entries.Select(entry => Task.Run(() =>
                {
                    entry.Texture = Texture2D.FromFile(game.GraphicsDevice, $"Content/img/{ src }/{ entry.Src }");
                    entry.FrameTime = 1000f / entry.FPS;

                    var size = entry.Texture.Size() / tileset.Size;
                    entry.Quads = new Rectangle[size.X, size.Y];
                    for (var x = 0; x < size.X; x++) {
                        for (var y = 0; y < size.Y; y++) {
                            entry.Quads[x, y] = new Rectangle(
                                new Point(x, y) * tileset.Size,
                                tileset.Size);
                        }
                    }
                })).ToList());
                return tileset;
            }
        }

        public void Run()
        {
            foreach (var i in sprites)
            {
                ref var sprite = ref sprites.Get1(i);
                if (sprite.Sheet?.Src != sprite.Src)
                {
                    if (sheets.TryGetValue(sprite.Src, out var task))
                    {
                        if (task.IsCompletedSuccessfully)
                            sprite.Sheet = task.Result;
                    }
                    else
                        sheets[sprite.Src] = LoadSheet(sprite.Src);
                }
            }

            foreach (var i in tiles)
            {
                ref var component = ref tiles.Get1(i);
                if (component.Tileset?.Src != component.Src)
                {
                    if (tilesets.TryGetValue(component.Src, out var task))
                    {
                        if (task.IsCompletedSuccessfully)
                            component.Tileset = task.Result;
                    }
                    else
                        tilesets[component.Src] = LoadTileset(component.Src);
                }
            }
        }
    }
}
