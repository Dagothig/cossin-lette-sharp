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
using System;
using System.Threading;

namespace Lette.Systems
{
    public class LoadEntry<T>
    {
        public readonly string Src;
        public readonly GenIdx Idx;
        public CancellationTokenSource? TokenSource;

        public LoadEntry(string src, GenIdx idx)
        {
            Src = src;
            Idx = idx;
        }
    }

    public class Loader : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        JsonSerializerOptions? options = null;
        FileSystemWatcher? watcher = null;

        Game? game = null;
        EcsFilter<Sprite>? sprites = null;
        EcsFilter<Tiles>? tiles = null;

        GenArr<Sheet>? sheets = null;
        GenArr<Tileset>? tilesets = null;
        Dictionary<string, LoadEntry<Sheet>> sheetsEntries = new();
        Dictionary<string, LoadEntry<Tileset>> tilesetsEntries = new();

        public void Init()
        {
            options = JsonSerialization.GetOptions();

            // TODO This should only be in debug
            watcher = new()
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
            watcher?.Dispose();
            watcher = null;
        }

        public void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.Name == null)
                return;
            e.Name.Split("/").Take(out var type, out var resource);

            if (type == "img")
            {
                if (sheetsEntries.TryGetValue(resource, out var sheetEntry) && sheets != null)
                    Load(sheets, sheetEntry, LoadSheet);
                else if (tilesetsEntries.TryGetValue(resource, out var tilesetEntry) && tilesets != null)
                    Load(tilesets, tilesetEntry, LoadTileset);
            }
        }

        public async Task<Sheet> LoadSheet(string src, CancellationToken token)
        {
            if (game == null)
                throw new Exception();
            using (var file = File.OpenRead($"Content/img/{ src }/sheet.json"))
            {
                var sheet = await JsonSerializer.DeserializeAsync<Sheet>(file, options, token);
                if (sheet == null)
                    throw new Exception();
                sheet.Src = src;
                await Task.WhenAll(sheet.Entries.Select(entry => Task.Run(() =>
                {
                    entry.Texture = Texture2D.FromFile(game.GraphicsDevice, $"Content/img/{ src }/{ entry.Src }");
                    if (token.IsCancellationRequested) return;
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

        public async Task<Tileset> LoadTileset(string src, CancellationToken token)
        {
            if (game == null)
                throw new Exception();
            using (var file = File.OpenRead($"Content/img/{ src }/tileset.json"))
            {
                var tileset = await JsonSerializer.DeserializeAsync<Tileset>(file, options, token);
                if (tileset == null)
                    throw new Exception();
                tileset.Src = src;
                await Task.WhenAll(tileset.Entries.Select(entry => Task.Run(() =>
                {
                    entry.Texture = Texture2D.FromFile(game.GraphicsDevice, $"Content/img/{ src }/{ entry.Src }");
                    if (token.IsCancellationRequested) return;
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

        public void Load<T>(GenArr<T> arr, LoadEntry<T> entry, Func<string, CancellationToken, Task<T>> loader)
        {
            entry.TokenSource?.Cancel();
            entry.TokenSource = new CancellationTokenSource();
            var token = entry.TokenSource.Token;
            loader(entry.Src, token).ContinueWith(async res =>
            {
                // TODO LOL maybe data race?
                if (!token.IsCancellationRequested)
                    arr[entry.Idx] = await res;
            });
        }

        public void Run()
        {
            if (sprites != null && sheets != null) foreach (var i in sprites)
            {
                ref var sprite = ref sprites.Get1(i);
                if (!sprite.SheetIdx.IsNull)
                    continue;
                if (sheetsEntries.TryGetValue(sprite.Src, out var foundEntry))
                {
                    sprite.SheetIdx = foundEntry.Idx;
                }
                else
                {
                    var entry = new LoadEntry<Sheet>(sprite.Src, sheets.Allocator.Alloc());
                    sprite.SheetIdx = entry.Idx;
                    sheetsEntries[sprite.Src] = entry;
                    Load(sheets, entry, LoadSheet);
                }
            }

            if (tiles != null && tilesets != null) foreach (var i in tiles)
            {
                ref var component = ref tiles.Get1(i);
                if (!component.TilesetIdx.IsNull)
                    continue;
                if (tilesetsEntries.TryGetValue(component.Src, out var foundEntry))
                {
                    component.TilesetIdx = foundEntry.Idx;
                }
                else
                {
                    var entry = new LoadEntry<Tileset>(component.Src, tilesets.Allocator.Alloc());
                    component.TilesetIdx = entry.Idx;
                    tilesetsEntries[component.Src] = entry;
                    Load(tilesets, entry, LoadTileset);
                }
            }
        }
    }
}
