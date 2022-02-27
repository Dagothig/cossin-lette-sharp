using System;
using FontStashSharp;
using Leopotam.Ecs;
using Lette.Components;
using Lette.Core;
using Lette.Editor;
using Lette.Resources;
using Lette.Systems;
using Gtk;
using System.Threading;

namespace Lette.States
{
    public class EditorState : EcsState
    {
        GenArr<Sheet>? sheets;
        GenArr<Tileset>? tilesets;
        GenArr<LevelDefinition>? levels;
        DynamicSpriteFont? font;

        Thread? uiThread;
        EditorWindow? window;

        EcsEntity? levelEntity;

        public override void InitSystems(out EcsSystems update, out EcsSystems draw, out EcsSystems systems)
        {
            sheets = new(new GenIdxAllocator());
            tilesets = new(new GenIdxAllocator());
            levels = new(new GenIdxAllocator());
            font = game?.Fonts?.GetFont(12);

            update = new EcsSystems(world)
                .Add(new SheetLoaderSystem())
                .Add(new TilesetLoaderSystem())
                .Add(new LevelLoaderSystem())
                .Add(new AABBSystem())
                .Add(new Animated());

            draw = new EcsSystems(world)
                .Add(new Renderer());

            systems = new EcsSystems(world)
                .Inject(sheets)
                .Inject(tilesets)
                .Inject(levels)
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
                .Replace(new Level() { Idx = levels!.Allocator.Alloc() });
        }

        public void StartUI()
        {
            Application.Init();
            window = new();
            window.LevelRef.OnChange += levelDef =>
            {
                ref var level = ref levelEntity!.Value.Get<Level>();
                level.Src = levelDef?.Src ?? string.Empty;
                levels![level.Idx] = levelDef;
            };
            window.EntityRef.OnChange += (entity) =>
            {
                
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
