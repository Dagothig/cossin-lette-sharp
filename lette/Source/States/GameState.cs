using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lette.Core;
using Lette.Systems;
using Leopotam.Ecs;
using Aether = tainicom.Aether.Physics2D;
using Lette.Components;
using Microsoft.Xna.Framework.Input;
using Lette.Resources;
using System.IO;
using System.Text.Json;
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
        SpriteBatch? batch;
        Aether.Dynamics.World? physicsWorld;
        DebugView? physicsDebugView;
        FileSystemWatcher? watcher;
        GenArr<Sheet>? sheets;
        GenArr<Tileset>? tilesets;
        TimeSpan step = TimeSpan.FromSeconds(1) / 60;

        public bool CapturesUpdate => true;

        public GameState(string initSrc = "Content/init.json")
        {
            this.initSrc = initSrc;
        }

        public void Init(CossinLette game)
        {
            this.game = game;
            batch = new(game.GraphicsDevice);
            physicsWorld = new(Vector2.Zero);
            physicsDebugView = new(physicsWorld);
            physicsDebugView.LoadContent(game.GraphicsDevice);
            watcher = new()
            {
                Path = "Content",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            sheets = new(new GenIdxAllocator());
            tilesets = new(new GenIdxAllocator());

            world = new();

            updateSystems = new EcsSystems(world)
                .Add(new SheetLoader())
                .Add(new TilesetLoader())
                .Add(new Inputs())
                .Add(new Actors())
                .Add(new Physics())
                .Add(new AABBs())
                .Add(new Animated());

            drawSystems = new EcsSystems(world)
                .Add(new Renderer())
                .Inject(batch);

            systems = new EcsSystems(world)
                .Add(updateSystems)
                .Add(drawSystems)
                .Inject(game)
                .Inject(watcher)
                .Inject(sheets)
                .Inject(tilesets)
                .Inject(physicsWorld)
                .Inject(physicsDebugView)
                .Inject(step);

            systems.Init();

            var level = JsonSerializer.Deserialize<LevelDefinition>(File.ReadAllText($"Content/levels/test.json"), JsonSerialization.GetOptions());
            if (level != null) foreach (var edef in level.Entities)
            {
                var entity = world.NewEntity();
                foreach (var component in edef)
                    component.Replace(entity);
            }
            var cossin = world
                .NewEntity()
                .Replace<Pos>(new Vector2(3, 3))
                .Replace(new Sprite { Src = "cossin" })
                .Replace(new Animator())
                .Replace(new Actor { Speed = 8 })
                .Replace(new Body { Shape = BodyShape.Circle(0.6f) })
                .Replace(new KeyMap
                {
                    Value = new()
                    {
                        { Keys.Left, (InputType.X, -1) },
                        { Keys.A, (InputType.X, -1) },
                        { Keys.Right, (InputType.X, 1) },
                        { Keys.D, (InputType.X, 1) },
                        { Keys.Up, (InputType.Y, -1) },
                        { Keys.W, (InputType.Y, -1) },
                        { Keys.Down, (InputType.Y, 1) },
                        { Keys.S, (InputType.Y, 1) }
                    }
                })
                .Replace(new Input() { Value = EnumArray<InputType, float>.New() })
                .Replace(new Camera());
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
            batch?.Dispose();
            watcher?.Dispose();
        }
    }
}
