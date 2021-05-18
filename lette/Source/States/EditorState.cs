using System;
using FontStashSharp;
using Leopotam.Ecs;
using Lette.Components;
using Lette.Core;
using Lette.Resources;
using Lette.Systems;
using Lette.Editor;

namespace Lette.States
{
    public class EditorState : EcsState
    {
        GenArr<Sheet>? sheets;
        GenArr<Tileset>? tilesets;
        GenArr<LevelDefinition>? levels;
        DynamicSpriteFont? font;
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
                .Add(new EditorWindow());

            draw = new EcsSystems(world)
                .Add(new Renderer());

            systems = new EcsSystems(world)
                .Inject(sheets)
                .Inject(tilesets)
                .Inject(levels)
                .Inject(font);
        }

        public void OnWindowDestroyed(object? sender, EventArgs e) => game?.Exit();

        public override void Init(CossinLette game)
        {
            base.Init(game);

            if (world == null)
                throw new Exception();

            var level = world
                .NewEntity()
                .Replace(new Level { Src = "test" });

            var camera = world
                .NewEntity()
                .Replace(new Camera())
                .Replace(new Pos());
        }
    }
}
