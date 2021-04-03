using System;
using System.Linq;
using Lette.Components;
using Leopotam.Ecs;
using Lette.Resources;

namespace Lette.Systems
{
    public class Animated : IEcsRunSystem
    {
        EcsFilter<Sprite, Animator> AnimatedSprites = null;
        TimeSpan step = TimeSpan.MinValue;

        public void Run()
        {
            foreach (var i in AnimatedSprites)
            {
                ref var sprite = ref AnimatedSprites.Get1(i);
                ref var animator = ref AnimatedSprites.Get2(i);

                var sheet = sprite.Sheet;
                var flags = animator.Flags;

                // TODO - This is bad

                var entryIdx = 0;
                SheetEntry entry = null;
                for (var ei = 0; ei < sprite.Sheet.Entries.Length; ei++)
                {
                    entry = sprite.Sheet.Entries[ei];
                    if (flags.Matches(sprite.Sheet.Entries[ei].Flags))
                    {
                        entryIdx = ei;
                        break;
                    }
                }

                var stripIdx = 0;
                for (var si = 0; si < entry.Strips.Length; si++)
                    if (flags.Matches(entry.Strips[si].Flags))
                    {
                        stripIdx = si;
                        break;
                    }

                if (sprite.Entry != entryIdx || sprite.Strip != stripIdx)
                {
                    animator.Time = 0;
                    sprite.Tile = 1;
                    sprite.Entry = entryIdx;
                    sprite.Strip = stripIdx;
                }
                else
                {
                    animator.Time += (float)step.TotalMilliseconds;
                    int shift = (int)(animator.Time / entry.FrameTime);
                    animator.Time -= shift * entry.FrameTime;
                    sprite.Tile = (sprite.Tile + shift) % entry.TilesCount;
                }
            }
        }
    }
}
