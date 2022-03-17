using System;
using System.Collections.Generic;
using System.Threading;
using FontStashSharp;
using Gtk;
using Leopotam.Ecs;
using Lette.Components;
using Lette.Core;
using Lette.Editor;
using Lette.Resources;
using Lette.Systems;

namespace Lette.States
{
    internal class EntityUpdaterSystem : IEcsRunSystem
    {
        Dictionary<string, List<(IReplaceOnEntity?, IReplaceOnEntity?)>> componentUpdates;
        EcsFilter<Id>? owned = null;

        public EntityUpdaterSystem(Dictionary<string, List<(IReplaceOnEntity?, IReplaceOnEntity?)>> componentUpdates)
        {
            this.componentUpdates = componentUpdates;
        }

        public void Run()
        {
            lock (componentUpdates)
                foreach (var i in owned!)
                {
                    ref var id = ref owned.Get1(i);
                    ref var entity = ref owned.GetEntity(i);
                    if (componentUpdates.TryGetValue(id.Value, out var updates))
                    {
                        foreach (var update in updates)
                        {
                            update.Item1?.Remove(entity);
                            update.Item2?.Replace(entity);
                        }
                        updates.Clear();
                        componentUpdates.Remove(id.Value);
                    }
                }
        }
    }

    public class EditorState : EcsState
    {
        GenArr<Sheet>? sheets;
        GenArr<Tileset>? tilesets;
        GenArr<LevelDefinition>? levels;
        DynamicSpriteFont? font;
        Dictionary<string, EcsEntity>? namedEntities;

        Thread? uiThread;
        EditorWindow? window;

        EcsEntity? levelEntity;

        public Dictionary<string, List<(IReplaceOnEntity?, IReplaceOnEntity?)>> ComponentUpdates = new();

        public override void InitSystems(out EcsSystems update, out EcsSystems draw, out EcsSystems systems)
        {
            sheets = new(new GenIdxAllocator());
            tilesets = new(new GenIdxAllocator());
            levels = new(new GenIdxAllocator());
            namedEntities = new(128);
            font = game?.Fonts?.GetFont(12);

            update = new EcsSystems(world)
                .Add(new SheetLoaderSystem())
                .Add(new TilesetLoaderSystem())
                .Add(new LevelLoaderSystem())
                .Add(new EntityUpdaterSystem(ComponentUpdates))
                .Add(new AABBSystem())
                .Add(new Animated());

            draw = new EcsSystems(world)
                .Add(new Renderer());

            systems = new EcsSystems(world)
                .Inject(sheets)
                .Inject(tilesets)
                .Inject(levels)
                .Inject(namedEntities)
                .Inject(font);
        }

        public override void Init(CossinLette game)
        {
            base.Init(game);

            if (world == null)
                throw new Exception();

            uiThread = new(new ThreadStart(StartUI));
            uiThread.Start();

            var camera = world
                .NewEntity()
                .Replace(new Camera())
                .Replace(new Pos());

            levelEntity = world
                .NewEntity()
                .Replace(new Level());
        }

        public void StartUI()
        {
            Application.Init();
            window = new();
            window.LevelRef.OnChange += levelDef =>
            {
                ref var level = ref levelEntity!.Value.Get<Level>();
                if (levelDef != null)
                {
                    if (!levels!.Allocator.Alive(level.Idx))
                        level.Idx = levels.Allocator.Alloc();
                    levels[level.Idx] = levelDef;
                    level.Src = levelDef.Src;
                }
                else
                {
                    levels!.Allocator.Dealloc(level.Idx);
                    level.Src = null;
                }

                levelEntity!.Value.Replace<Level>(new() { Src = levelDef?.Src });
            };
            window.History.OnApply += (command) =>
            {
                switch (command)
                {
                    case CRUDComponentCommand cmd:
                        lock (ComponentUpdates)
                            ComponentUpdates.GetOrCreate(cmd.Entity.Item1).Add((cmd.OldValue, cmd.NewValue));
                        break;
                    default:
                        Console.WriteLine($"Unsupported command { command.Description }");
                        break;
                }
            };
            window.History.OnUndo += (command) =>
            {
                switch (command)
                {
                    case CRUDComponentCommand cmd:
                        lock (ComponentUpdates)
                            ComponentUpdates.GetOrCreate(cmd.Entity.Item1).Add((cmd.NewValue, cmd.OldValue));
                        break;
                    default:
                        Console.WriteLine($"Unsupported command { command.Description }");
                        break;
                }
            };
            Application.Run();
        }

        public override void Update()
        {
            if (!uiThread?.IsAlive ?? true)
                game?.Exit();

            base.Update();
        }

        public override void Destroy()
        {
            if (uiThread?.IsAlive ?? false)
                Application.Quit();

            base.Destroy();
        }
    }
}
