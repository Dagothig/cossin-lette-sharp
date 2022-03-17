using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Leopotam.Ecs;
using Lette.Components;
using Lette.Core;
using Lette.Core.JsonSerialization;
using Lette.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tile = Lette.Resources.Tile;

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

    public abstract class LoaderSystem<H, R> : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    where H : struct, IHandle
    {
        protected JsonSerializerOptions options = JsonSerialization.Options;
        protected FileSystemWatcher? watcher = null;

        protected Game? game = null;
        protected EcsFilter<H>? handles = null;

        protected GenArr<R>? resources = null;
        protected bool usedEntriesRunValue = false;
        protected bool[]? usedEntries = null;
        protected Stack<string> unusedEntryKeys = new();
        protected Dictionary<string, LoadEntry<R>> entries = new();

        public abstract string Folder { get; }
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
                catch (OperationCanceledException) { }
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
            usedEntries = new bool[resources!.Backing.Length];
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
            (e.Name ?? "").Split("/").Take(out var type, out var raw_resource);
            raw_resource.Split(".").Take(out var resource);
            if (type == Folder && entries.TryGetValue(resource, out var entry) && resources != null)
                Load(entry, true);
        }

        public virtual void Run()
        {
            usedEntriesRunValue = !usedEntriesRunValue;
            foreach (var i in handles!)
            {
                ref var handle = ref handles.Get1(i);
                if (!handle.Idx.IsNull)
                    continue;
                if (handle.Src != null && entries.TryGetValue(handle.Src, out var foundEntry))
                {
                    handle.Idx = foundEntry.Idx;
                }
                else if (handle.Src != null)
                {
                    var entry = new LoadEntry<R>(handle.Src, resources!.Allocator.Alloc());
                    handle.Idx = entry.Idx;
                    entries[handle.Src] = entry;
                    Load(entry);
                }
                usedEntries![handle.Idx.Index] = usedEntriesRunValue;
            }
            // Deallocate unused resources
            for (var i = 0; i < usedEntries!.Length; i++)
                if (usedEntries[i] != usedEntriesRunValue)
                {
                    var entry = resources!.Allocator.Entries[i];
                    if (entry.Alive)
                    {
                        var index = new GenIdx { Index = i, Generation = entry.Generation };
                        resources[index] = default(R);
                        resources.Allocator.Dealloc(index);
                    }
                    usedEntries[i] = usedEntriesRunValue;
                }
            // Remove dangling entries using a stack because you can't modify a dictionary while iterating through it.
            unusedEntryKeys.PushAll(entries
                .Where(e => !resources!.Allocator.Alive(e.Value.Idx))
                .Select(e => e.Key));
            entries.RemoveAll(unusedEntryKeys);
            unusedEntryKeys.Clear();
        }
    }

    public class SheetLoaderSystem : LoaderSystem<Sprite, Sheet>
    {
        public override string Folder => "img";

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

    public class TilesetLoaderSystem : LoaderSystem<Tiles, Tileset>
    {
        public override string Folder => "img";

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
                    for (var x = 0; x < size.X; x++)
                    {
                        for (var y = 0; y < size.Y; y++)
                        {
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

    public class LevelLoaderSystem : LoaderSystem<Level, LevelDefinition>
    {
        public override string Folder => "levels";

        EcsFilter<Owner>? owned = null;
        EcsFilter<Id>? named = null;
        EcsWorld? world = null;
        Dictionary<string, EcsEntity>? namedEntities = null;

        public override async Task<LevelDefinition> Load(string src, CancellationToken token)
        {
            if (game == null)
                throw new NullReferenceException();
            using (var file = File.OpenRead($"Content/levels/{ src }.json"))
            {
                var def = await JsonSerializer.DeserializeAsync<LevelDefinition>(file, options, token);
                if (def == null)
                    throw new Exception();
                def.Src = src;
                return def;
            }
        }

        public override void Run()
        {
            base.Run();

            if (handles == null || owned == null || world == null || resources == null || named == null || namedEntities == null)
                return;

            foreach (var i in handles)
            {
                ref var level = ref handles.Get1(i);
                var def = resources?[level.Idx];
                if (def == null)
                    continue;

                var hash = def.GetHashCode();
                if (level.DefHash == hash)
                    continue;

                var levelEntity = handles.GetEntity(i);
                // Recreate entities
                if (level.DefHash != null)
                {
                    foreach (var j in owned)
                    {
                        ref var owner = ref owned.Get1(j);
                        if (owner.Value == levelEntity)
                            owned.GetEntity(j).Destroy();
                    }
                }
                // Create new entities
                foreach (var (id, edef) in def.Entities)
                {
                    var entity = world
                        .NewEntity()
                        .Replace<Owner>(levelEntity)
                        .Replace<Id>(id);
                    foreach (var component in edef)
                        component.Replace(entity);
                }
                level.DefHash = hash;
            }

            // Remove orphans
            foreach (var i in owned)
            {
                ref var owner = ref owned.Get1(i);
                if (!owner.Value.IsAlive())
                {
                    ref var entity = ref owned.GetEntity(i);
                    entity.Destroy();
                }
            }

            namedEntities.Clear();
            foreach (var i in named)
                namedEntities[named.Get1(i)] = named.GetEntity(i);
        }
    }
}
