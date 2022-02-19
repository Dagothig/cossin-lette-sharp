using System;
using Leopotam.Ecs;
using Aether = tainicom.Aether.Physics2D;
using Lette.Components;
using Lette.Core;
using tainicom.Aether.Physics2D.Common;

namespace Lette.Systems
{
    public class PhysicsSystem : IEcsRunSystem
    {
        Aether.Dynamics.World? world = null;
        TimeSpan step = TimeSpan.MinValue;
        EcsFilter<Body, Pos>? bodies = null;
        EcsFilter<Actor, Body>? actorBodies = null;
        EcsFilter<StaticCollisions>? staticCollisions = null;

        public void Run()
        {
            if (world == null)
                return;

            world.Step(step);

            if (bodies != null) foreach (var i in bodies!)
            {
                ref var entity = ref bodies.GetEntity(i);
                ref var body = ref bodies.Get1(i);

                if (body.Physics == null)
                {
                    ref var pos = ref bodies.Get2(i);
                    Aether.Dynamics.Body physicsBody;

                    switch (body.Shape.Type)
                    {
                        case BodyShapeType.Circle:
                            physicsBody = world.CreateCircle(
                                body.Shape.Radius,
                                1,
                                pos,
                                Aether.Dynamics.BodyType.Dynamic);
                            break;
                        default:
                            continue;
                    }

                    body.Physics = physicsBody;
                    body.Physics.Tag = entity;
                    physicsBody.LinearDamping = Constants.DAMPING;
                    physicsBody.FixedRotation = true;
                }

                if (body.Physics.Awake)
                    entity.Replace<Pos>(body.Physics.Position);
            }

            if (actorBodies != null) foreach (var i in actorBodies)
            {
                ref var actor = ref actorBodies.Get1(i);
                ref var body = ref actorBodies.Get2(i);

                if (body.Physics?.Awake ?? false)
                    body.Physics.Rotation = actor.Flags.Angle();
            }

            if (staticCollisions != null) foreach (var i in staticCollisions)
            {
                ref var entity = ref staticCollisions.GetEntity(i);
                ref var col = ref entity.Get<StaticCollisions>();
                ref var pos = ref entity.Get<Pos>();

                if (col.Physics == null)
                {
                    col.Physics = world.CreateBody(pos);
                    col.Physics.Tag = entity;

                    foreach (var chain in col.Chains)
                        col.Physics.CreateChainShape(new Vertices(chain));
                }
            }
        }
    }
}
