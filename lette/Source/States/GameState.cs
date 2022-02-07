using System;
using System.IO;
using System.Text.Json;
using FontStashSharp;
using Leopotam.Ecs;
using Lette.Components;
using Lette.Core;
using Lette.Core.JsonSerialization;
using Lette.Lib.Physics;
using Lette.Resources;
using Lette.Systems;
using Microsoft.Xna.Framework;
using Aether = tainicom.Aether.Physics2D;

namespace Lette.States
{
    public class GameState : EcsState
    {
        Aether.Dynamics.World? physicsWorld;
        DebugView? physicsDebugView;
        GenArr<Sheet>? sheets;
        GenArr<Tileset>? tilesets;
        GenArr<LevelDefinition>? levels;
        DynamicSpriteFont? font;

        public override void InitSystems(out EcsSystems update, out EcsSystems draw, out EcsSystems systems)
        {
            if (game == null)
                throw new Exception();

            physicsWorld = new(Vector2.Zero);
            physicsDebugView = new(physicsWorld);
            physicsDebugView.LoadContent(game.GraphicsDevice, game.Fonts);
            sheets = new(new GenIdxAllocator());
            tilesets = new(new GenIdxAllocator());
            levels = new(new GenIdxAllocator());
            font = game.Fonts?.GetFont(12);

            update = new EcsSystems(world)
                .Add(new SheetLoaderSystem())
                .Add(new TilesetLoader())
                .Add(new LevelLoaderSystem())
                .Add(new InputSystem())
                .Add(new ActorSystem())
                .Add(new PhysicsSystem())
                .Add(new AABBSystem())
                .Add(new AnimatedSystem());

            draw = new EcsSystems(world)
                .Add(new RendererSystem());

            systems = new EcsSystems(world)
                .Inject(sheets)
                .Inject(tilesets)
                .Inject(levels)
                .Inject(physicsWorld)
                .Inject(physicsDebugView)
                .Inject(font);
        }

        public override void Init(CossinLette game)
        {
            base.Init(game);

            if (world == null)
                throw new Exception();

            var init = JsonSerializer.Deserialize<Init>(
                File.ReadAllText($"Content/init.json"),
                JsonSerialization.Options);
            if (init == null)
                throw new Exception();

            var level = world
                .NewEntity()
                .Replace(new Level { Src = init.Level });

            var player = world.NewEntity().Replace<Id>("player");
            foreach (var component in init.Player)
                component.Replace(player);
        }
    }
}
