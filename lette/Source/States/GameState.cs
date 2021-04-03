using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lette.Core;
using Lette.Systems;
using Leopotam.Ecs;
using Aether = tainicom.Aether.Physics2D;
using Lette.Components;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Lette.Resources;

namespace Lette.States
{
    public class GameState : IState
    {
        EcsWorld world;
        EcsSystems updateSystems;
        EcsSystems drawSystems;
        EcsSystems systems;

        CossinLette game;
        SpriteBatch batch;
        SpatialMap<EcsEntity> spatialMap;
        Aether.Dynamics.World physicsWorld;
        TimeSpan step = TimeSpan.FromSeconds(1) / 60;

        public bool CapturesUpdate => true;

        public GameState(CossinLette game)
        {
            this.game = game;
        }

        public void Init(Game game)
        {
            batch = new SpriteBatch(game.GraphicsDevice);
            spatialMap = new SpatialMap<EcsEntity>(128);
            physicsWorld = new Aether.Dynamics.World(Vector2.Zero);

            world = new EcsWorld();

            updateSystems = new EcsSystems(world)
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
                .Inject(physicsWorld)
                .Inject(spatialMap)
                .Inject(step);

            systems.Init();

            // TEST
            var sheet = new Sheet
            {
                Src = "cossin",
                Entries = new SheetEntry[]
                {
                    new SheetEntry
                    {
                        Src = "cossin-marche",
                        Size = new Vector2(64, 96),
                        Decal = new Vector2(32, 80),
                        FPS = 10,
                        Flags = AnimFlag.Moving,
                        StripsVariants = Sheet.FlipDirStripsVariants,
                    },
                    new SheetEntry
                    {
                        Src = "cossin-neutre",
                        Size = new Vector2(64, 96),
                        Decal = new Vector2(32, 80),
                        FPS = 4,
                        StripsVariants = Sheet.FlipDirStripsVariants,
                    }
                }
            };
            sheet.Init(game);

            /*var other = world
                .NewEntity()
                .Replace<Pos>(new Vector2(2, 2))
                .Replace(new Sprite
                {
                    Src = "cossin",
                    Sheet = sheet
                })
                .Replace(new Animator())
                .Replace(new Actor { Speed = 8, Flags = AnimFlag.DirBR })
                .Replace(new Body { Shape = BodyShape.Circle(0.6f) })
                .Replace(new Camera());*/

            var cossin = world
                .NewEntity()
                .Replace<Pos>(new Vector2(3, 3))
                .Replace(new Sprite
                {
                    Src = "cossin",
                    Sheet = sheet
                })
                .Replace(new Animator())
                .Replace(new Actor { Speed = 8 })
                .Replace(new Body { Shape = BodyShape.Circle(0.6f) })
                .Replace(new KeyMap
                {
                    Value = new Dictionary<Keys, (InputType, float)>
                    {
                        { Keys.Left, (InputType.X, -1) },
                        { Keys.Right, (InputType.X, 1) },
                        { Keys.Up, (InputType.Y, -1) },
                        { Keys.Down, (InputType.Y, 1) }
                    }
                })
                .Replace(new Input()
                {
                    Value = EnumArray<InputType, float>.New()
                })
                .Replace(new Camera());
        }

        public void Update()
        {
            updateSystems.Run();
        }
        public void Draw()
        {
            drawSystems.Run();
        }

        public void Destroy()
        {
            systems.Destroy();
            world.Destroy();
            batch.Dispose();
        }
    }
}
