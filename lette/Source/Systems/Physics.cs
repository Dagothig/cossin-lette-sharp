using System;
using Leopotam.Ecs;
using Aether = tainicom.Aether.Physics2D;
using Lette.Components;
using Lette.Core;

namespace Lette.Systems
{
    public class Physics : IEcsInitSystem, IEcsDestroySystem, IEcsRunSystem
    {
        public class BodiesListener : IEcsFilterListener
        {
            internal Physics physics;

            public void OnEntityAdded(in EcsEntity entity)
            {
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
            {
                ref var body = ref entity.Get<Body>();
                if (body.Physics != null)
                {
                    physics.world.Remove(body.Physics);
                    body.Physics.Tag = null;
                }
            }
        }

        Aether.Dynamics.World world = null;
        TimeSpan step = TimeSpan.MinValue;
        EcsFilter<Body, Pos> bodies = null;
        EcsFilter<Actor, Body> actorBodies = null;
        BodiesListener bodiesListener = null;

        public void Init()
        {
            bodiesListener = new BodiesListener() { physics = this };
            bodies.AddListener(bodiesListener);
        }

        public void Destroy()
        {
            bodies.RemoveListener(bodiesListener);
        }

        public void Run()
        {
            world.Step(step);

            foreach (var i in bodies)
            {
                ref var entity = ref bodies.GetEntity(i);
                ref var body = ref bodies.Get1(i);
                if (body.Physics?.Awake ?? false)
                    entity.Replace<Pos>(body.Physics.Position);
            }

            foreach (var i in actorBodies)
            {
                ref var actor = ref actorBodies.Get1(i);
                ref var body = ref actorBodies.Get2(i);

                if (body.Physics?.Awake ?? false)
                    body.Physics.Rotation = actor.Flags.Angle();
            }
        }
    }
}
