using System;
using Leopotam.Ecs;
using Aether = tainicom.Aether.Physics2D;
using Lette.Components;
using Lette.Core;
using tainicom.Aether.Physics2D.Common;

namespace Lette.Systems
{
    public class PhysicsSystem : IEcsInitSystem, IEcsDestroySystem, IEcsRunSystem
    {
        public class BodiesListener : IEcsFilterListener
        {
            internal PhysicsSystem physics;

            public BodiesListener(PhysicsSystem physics)
            {
                this.physics = physics;
            }

            public void OnEntityAdded(in EcsEntity entity)
            {
                if (physics.world == null)
                    return;

                ref var body = ref entity.Get<Body>();
                ref var pos = ref entity.Get<Pos>();
                Aether.Dynamics.Body physicsBody;

                switch (body.Shape.Type)
                {
                    case BodyShapeType.Circle:
                        physicsBody = physics.world.CreateCircle(
                            body.Shape.Radius,
                            1,
                            pos,
                            Aether.Dynamics.BodyType.Dynamic);
                        break;
                    default:
                        return;
                }

                body.Physics = physicsBody;
                body.Physics.Tag = entity;
                physicsBody.LinearDamping = Constants.DAMPING;
                physicsBody.FixedRotation = true;
            }

            public void OnEntityRemoved(in EcsEntity entity)
            { }
        }

        public class StaticCollisionsListener : IEcsFilterListener
        {
            internal PhysicsSystem physics;

            public StaticCollisionsListener(PhysicsSystem physics)
            {
                this.physics = physics;
            }

            public void OnEntityAdded(in EcsEntity entity)
            {
                if (physics.world == null)
                    return;

                ref var staticCollisions = ref entity.Get<StaticCollisions>();
                ref var pos = ref entity.Get<Pos>();

                staticCollisions.Physics = physics.world.CreateBody(pos);
                staticCollisions.Physics.Tag = entity;

                foreach (var chain in staticCollisions.Chains)
                    staticCollisions.Physics.CreateChainShape(new Vertices(chain));
            }

            public void OnEntityRemoved(in EcsEntity entity)
            {}
        }

        Aether.Dynamics.World? world = null;
        TimeSpan step = TimeSpan.MinValue;
        EcsFilter<Body, Pos>? bodies = null;
        EcsFilter<Actor, Body>? actorBodies = null;
        EcsFilter<StaticCollisions>? staticCollisions = null;

        BodiesListener? bodiesListener = null;
        StaticCollisionsListener? staticCollisionsListener = null;

        public void Init()
        {
            bodiesListener = new BodiesListener(this);
            bodies?.AddListener(bodiesListener);

            staticCollisionsListener = new StaticCollisionsListener(this);
            staticCollisions?.AddListener(staticCollisionsListener);
        }

        public void Destroy()
        {
            bodies?.RemoveListener(bodiesListener);
        }

        public void Run()
        {
            world?.Step(step);

            if (bodies != null) foreach (var i in bodies)
            {
                ref var entity = ref bodies.GetEntity(i);
                ref var body = ref bodies.Get1(i);
                if (body.Physics?.Awake ?? false)
                    entity.Replace<Pos>(body.Physics.Position);
            }

            if (actorBodies != null) foreach (var i in actorBodies)
            {
                ref var actor = ref actorBodies.Get1(i);
                ref var body = ref actorBodies.Get2(i);

                if (body.Physics?.Awake ?? false)
                    body.Physics.Rotation = actor.Flags.Angle();
            }
        }
    }
}
