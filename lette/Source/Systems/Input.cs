using System.Linq;
using Lette.Components;
using Leopotam.Ecs;
using Microsoft.Xna.Framework.Input;

namespace Lette.Systems
{
    public class Inputs : IEcsRunSystem
    {
        EcsFilter<Input> inputs = null;
        EcsFilter<KeyMap, Input> keysInputs = null;

        public void Run()
        {
            foreach (var i in inputs)
            {
                ref var input = ref inputs.Get1(i);

                foreach (var type in input.Value.Keys)
                    input.Value[type] = 0;
            }

            var keyboardState = Keyboard.GetState();
            foreach (var i in keysInputs)
            {
                ref var keyMap = ref keysInputs.Get1(i);
                ref var input = ref keysInputs.Get2(i);

                foreach (var (key, (type, value)) in keyMap.Value.Entries)
                    if (keyboardState.IsKeyDown(key))
                        input.Value[type] += value;
            }
        }
    }
}
