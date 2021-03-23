using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lette.Core;
using Lette.Systems;
using Leopotam.Ecs;
using Aether = tainicom.Aether.Physics2D;

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
            spatialMap = new SpatialMap<EcsEntity>(1);
            physicsWorld = new Aether.Dynamics.World();

            world = new EcsWorld();

            updateSystems = new EcsSystems(world)
                .Add(new Inputs())
                .Add(new Actors())
                .Add(new Physics())
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
