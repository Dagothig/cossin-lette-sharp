using Lette.Components;
using Lette.Core;
using Leopotam.Ecs;

using static System.MathF;

namespace Lette.Systems
{
    public class ActorSystem : IEcsRunSystem
    {
        EcsFilter<Actor, Input>? actorInputs = null;
        EcsFilter<Actor, Animator>? actorAnimators = null;
        EcsFilter<Actor, Body>? actorBodies = null;

        public void Run()
        {
            if (actorInputs != null) foreach (var i in actorInputs)
            {
                ref var actor = ref actorInputs.Get1(i);
                ref var input = ref actorInputs.Get2(i);

                if (input.Value[InputType.X] == 0 && input.Value[InputType.Y] == 0)
                    actor.Flags[AnimFlag.Moving] = false;
                else
                {
                    actor.Flags[AnimFlag.Moving] = true;
                    actor.Flags.SetAngle(Atan2(
                        input.Value[InputType.Y],
                        input.Value[InputType.X]));
                }
            }

            if (actorAnimators != null) foreach (var i in actorAnimators)
            {
                ref var actor = ref actorAnimators.Get1(i);
                ref var animator = ref actorAnimators.Get2(i);

                animator.Flags = actor.Flags;
            }

            if (actorBodies != null) foreach (var i in actorBodies)
            {
                ref var actor = ref actorBodies.Get1(i);
                ref var body = ref actorBodies.Get2(i);

                if (actor.Flags[AnimFlag.Moving])
                {
                    body.Physics?.ApplyForce(
                        actor.Flags.Angle().V2() *
                        actor.Speed *
                        Pow(Constants.DAMPING, 1.25f));
                }
            }
        }
    }
}
