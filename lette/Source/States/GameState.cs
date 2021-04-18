using Microsoft.Xna.Framework;
using Lette.Core;
using Lette.Systems;
using Leopotam.Ecs;
using Aether = tainicom.Aether.Physics2D;
using Lette.Components;
using Microsoft.Xna.Framework.Input;
using Lette.Resources;
using Lette.Lib.Physics;

namespace Lette.States
{
    public class GameState : IState
    {
        string initSrc;

        EcsWorld? world;
        EcsSystems? updateSystems;
        EcsSystems? drawSystems;
        EcsSystems? systems;

        CossinLette? game;
        Aether.Dynamics.World? physicsWorld;
        DebugView? physicsDebugView;
        GenArr<Sheet>? sheets;
        GenArr<Tileset>? tilesets;
        GenArr<LevelDefinition>? levels;

        public bool CapturesUpdate => true;

        public GameState(string initSrc = "Content/init.json")
        {
            this.initSrc = initSrc;
        }

        public void Init(CossinLette game)
        {
            this.game = game;
            physicsWorld = new(Vector2.Zero);
            physicsDebugView = new(physicsWorld);
            physicsDebugView.LoadContent(game.GraphicsDevice, game.Fonts);
            sheets = new(new GenIdxAllocator());
            tilesets = new(new GenIdxAllocator());
            levels = new(new GenIdxAllocator());

            world = new();

            updateSystems = new EcsSystems(world)
                .Add(new SheetLoader())
                .Add(new TilesetLoader())
                .Add(new LevelLoader())
                .Add(new Inputs())
                .Add(new Actors())
                .Add(new Physics())
                .Add(new AABBs())
                .Add(new Animated());

            drawSystems = new EcsSystems(world)
                .Add(new Renderer())
                .Inject(game.Batch)
                .Inject(game.Fonts);

            systems = new EcsSystems(world)
                .Add(updateSystems)
                .Add(drawSystems)
                .Inject(game)
                .Inject(game.Watcher)
                .Inject(sheets)
                .Inject(tilesets)
                .Inject(levels)
                .Inject(game.Step)
                .Inject(physicsWorld)
                .Inject(physicsDebugView);

            systems.Init();

            if (game.Init == null)
                return;

            var level = world
                .NewEntity()
                .Replace(new Level { Src = game.Init.Level });

            var player = world.NewEntity().Replace<Id>("player");
            foreach (var component in game.Init.Player)
                component.Replace(player);
        }

        public void Update()
        {
            updateSystems?.Run();
        }
        public void Draw()
        {
            drawSystems?.Run();
        }

        public void Destroy()
        {
            systems?.Destroy();
            world?.Destroy();
        }
    }
}
