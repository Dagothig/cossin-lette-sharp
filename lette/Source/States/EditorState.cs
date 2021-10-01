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
    public interface IAction
    {

    }

    public class EditorWindowUpdater : IEcsRunSystem
    {
        EditorWindow? window = null;

        public void Run()
        {
            if (window == null)
                return;
        }
    }

    public class EditorState : EcsState
    {
        GenArr<Sheet>? sheets;
        GenArr<Tileset>? tilesets;
        GenArr<LevelDefinition>? levels;
        DynamicSpriteFont? font;

        Thread? uiThread;
        EditorWindow? window;

        public override void InitSystems(out EcsSystems update, out EcsSystems draw, out EcsSystems systems)
        {
            sheets = new(new GenIdxAllocator());
            tilesets = new(new GenIdxAllocator());
            levels = new(new GenIdxAllocator());
            font = game?.Fonts?.GetFont(12);

            update = new EcsSystems(world)
                .Add(new SheetLoader())
                .Add(new TilesetLoader())
                .Add(new LevelLoader())
                .Add(new AABBs())
                .Add(new Animated())
                .Add(new EditorWindowUpdater());

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

            var level = world
                .NewEntity()
                .Replace(new Level { Src = "test" });

            var camera = world
                .NewEntity()
                .Replace(new Camera())
                .Replace(new Pos());
        }

        public void StartUI()
        {
            Application.Init();
            window = new();
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
