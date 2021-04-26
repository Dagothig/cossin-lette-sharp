using System;
using System.Threading.Tasks;
using Gtk;
using Leopotam.Ecs;
using Lette.Components;
using Lette.Core;
using Lette.Resources;
using Lette.Systems;

namespace Lette.States
{
    public class EditorState : EcsState
    {
        GenArr<Sheet>? sheets;
        GenArr<Tileset>? tilesets;
        GenArr<LevelDefinition>? levels;
        Task? windows;

        public override void InitSystems(out EcsSystems update, out EcsSystems draw, out EcsSystems systems)
        {
            if (game == null)
                throw new Exception();

            sheets = new(new GenIdxAllocator());
            tilesets = new(new GenIdxAllocator());
            levels = new(new GenIdxAllocator());

            update = new EcsSystems(world)
                .Add(new SheetLoader())
                .Add(new TilesetLoader())
                .Add(new LevelLoader())
                .Add(new AABBs())
                .Add(new Animated());

            draw = new EcsSystems(world)
                .Add(new Renderer());

            systems = new EcsSystems(world)
                .Inject(sheets)
                .Inject(tilesets)
                .Inject(levels);
        }

        public override void Init(CossinLette game)
        {
            base.Init(game);

            Application.Init();

            Window test = new("Cossin Lette");

            Label lbl = new();
            lbl.Text = "Buonjour";

            test.Add(lbl);

            test.ShowAll();

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

        public override void Update()
        {
            base.Update();
            Application.RunIteration(false);
        }
    }
}
