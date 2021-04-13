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

    public abstract class Loader<H, R> : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    where H: struct, IHandle
    {
        protected JsonSerializerOptions options = JsonSerialization.GetOptions();
        protected FileSystemWatcher? watcher = null;

        protected Game? game = null;
        protected EcsFilter<H>? handles = null;

        protected GenArr<R>? resources = null;
        protected Dictionary<string, LoadEntry<R>> entries = new();

        public abstract Task<R> Load(string src, CancellationToken token);

        public void Load(LoadEntry<R> entry, bool debounce = false)
        {
            if (resources == null)
                throw new NullReferenceException();
            entry.TokenSource?.Cancel();
            entry.TokenSource = new CancellationTokenSource();
            var token = entry.TokenSource.Token;
            Task.Run(async () =>
            {
                if (debounce)
                    await Task.Delay(50, token);
                try
                {
                    var result = await Load(entry.Src, token);
                    if (!token.IsCancellationRequested)
                        resources[entry.Idx] = result;
                }
                catch (OperationCanceledException) {}
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not load resource { entry.Src }");
                    Console.WriteLine(ex.ToString());
                }
            });
        }

        public void Init()
        {
            if (watcher != null)
                watcher.Changed += OnChanged;
        }

        public void Destroy()
        {
            foreach (var (key, value) in entries)
                value.TokenSource?.Cancel();
            if (watcher != null)
                watcher.Changed -= OnChanged;
        }

        public void OnChanged(object sender, FileSystemEventArgs e)
        {
            (e.Name ?? "").Split("/").Take(out var type, out var resource);
            if (type == "img" && entries.TryGetValue(resource, out var entry) && resources != null)
                Load(entry, true);
        }

        public void Run()
        {
            if (handles != null && entries != null && resources != null) foreach (var i in handles)
            {
                ref var handle = ref handles.Get1(i);
                if (!handle.Idx.IsNull)
                    continue;
                if (entries.TryGetValue(handle.Src, out var foundEntry))
                {
                    handle.Idx = foundEntry.Idx;
                }
                else
                {
                    var entry = new LoadEntry<R>(handle.Src, resources.Allocator.Alloc());
                    handle.Idx = entry.Idx;
                    entries[handle.Src] = entry;
                    Load(entry);
                }
            }
        }
    }

    public class SheetLoader : Loader<Sprite, Sheet>
    {
        public override async Task<Sheet> Load(string src, CancellationToken token)
        {
            if (game == null)
                throw new NullReferenceException();
            using (var file = File.OpenRead($"Content/img/{ src }/sheet.json"))
            {
                var sheet = await JsonSerializer.DeserializeAsync<Sheet>(file, options, token);
                if (sheet == null)
                    throw new Exception();
                sheet.Src = src;
                await Task.WhenAll(sheet.Entries.Select(entry => Task.Run(() =>
                {
                    entry.Texture = Texture2D.FromFile(game.GraphicsDevice, $"Content/img/{ src }/{ entry.Src }");
                    token.ThrowIfCancellationRequested();
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
    }

    public class TilesetLoader : Loader<Tiles, Tileset>
    {
        public override async Task<Tileset> Load(string src, CancellationToken token)
        {
            if (game == null)
                throw new NullReferenceException();
            using (var file = File.OpenRead($"Content/img/{ src }/tileset.json"))
            {
                var tileset = await JsonSerializer.DeserializeAsync<Tileset>(file, options, token);
                if (tileset == null)
                    throw new Exception();
                tileset.Src = src;
                await Task.WhenAll(tileset.Entries.Select(entry => Task.Run(() =>
                {
                    entry.Texture = Texture2D.FromFile(game.GraphicsDevice, $"Content/img/{ src }/{ entry.Src }");
                    token.ThrowIfCancellationRequested();
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
    }

    public class LevelLoader : IEcsRunSystem
    {
        EcsWorld? world;

        public void Run()
        {
            LevelDefinition ldef = new();

            if (world != null) foreach (var edef in ldef.Entities)
            {
                var entity = world.NewEntity();
                foreach (var component in edef)
                    component.Replace(entity);
            }
        }
    }
}
