using System;
using Lette.Components;
using Lette.Core;
using Leopotam.Ecs;

namespace Lette.Systems
{
    public class Actors : IEcsRunSystem
    {
        EcsFilter<Actor, Input> actorInputs = null;
        EcsFilter<Actor, Animator> actorAnimators = null;
        EcsFilter<Actor, Body> actorBodies = null;

        public void Run()
        {
            foreach (var i in actorInputs)
            {
                ref var actor = ref actorInputs.Get1(i);
                ref var input = ref actorInputs.Get2(i);

                if (input.Value[InputType.X] == 0 && input.Value[InputType.Y] == 0)
                    actor.Flags[AnimFlag.Moving] = false;
                else
                {
                    actor.Flags[AnimFlag.Moving] = true;
                    actor.Flags.SetAngle(MathF.Atan2(
                        input.Value[InputType.Y],
                        input.Value[InputType.X]));
                }
            }

            foreach (var i in actorAnimators)
            {
                ref var actor = ref actorAnimators.Get1(i);
                ref var animator = ref actorAnimators.Get2(i);

                animator.Flags = actor.Flags;
            }

            foreach (var i in actorBodies)
            {
                ref var actor = ref actorBodies.Get1(i);
                ref var body = ref actorBodies.Get2(i);

                body.Physics?.ApplyForce(
                    actor.Flags.Angle().V2() *
                    actor.Speed *
                    Constants.DAMPING);
            }
        }
    }
}
