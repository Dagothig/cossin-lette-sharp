using System.Linq;
using Lette.Components;
using Leopotam.Ecs;

namespace Lette.Systems
{
    public class Sheets : IEcsRunSystem
    {
        EcsWorld World;

        EcsFilter<Sprite, Animator> AnimatedSprites;

        public void Run()
        {
            foreach (var i in AnimatedSprites)
            {
                ref var sprite = ref AnimatedSprites.Get1(i);
                ref var animator = ref AnimatedSprites.Get2(i);

                // TODO: Lol
                var dt = 16f;
                var sheet = sprite.Sheet;
                var flags = animator.Flags;

                var entryIdx = sprite.Sheet
                    .Entries
                    .Where(e => flags.Matches(e.Flags))
                    .Select((_, index) => index)
                    .FirstOrDefault();

                var stripIdx = sprite.Sheet
                    .Entries[entryIdx]
                    .Strips
                    .Where(s => flags.Matches(s.Flags))
                    .Select((_, index) => index)
                    .FirstOrDefault();

                if (sprite.Entry != entryIdx || sprite.Strip != stripIdx)
                {
                    animator.Time = 0;
                    sprite.Tile = 1;
                    sprite.Entry = entryIdx;
                    sprite.Strip = stripIdx;
                }
                else
                {
                    animator.Time -= dt;
                    var entry = sheet.Entries[entryIdx];
                    int shift = (int)(animator.Time / entry.FrameTime);
                    animator.Time -= shift * entry.FrameTime;
                    sprite.Tile = (sprite.Tile + shift) % entry.TilesCount;
                }
            }
        }
    }
}
