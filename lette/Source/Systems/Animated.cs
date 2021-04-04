using System;
using Lette.Components;
using Leopotam.Ecs;
using Lette.Resources;

namespace Lette.Systems
{
    public class Animated : IEcsRunSystem
    {
        EcsFilter<Sprite, Animator> animatedSprites = null;
        EcsFilter<Tiles> animatedTiles = null;
        TimeSpan step = TimeSpan.MinValue;

        public void Run()
        {
            var dt = (float)step.TotalMilliseconds;

            foreach (var i in animatedSprites)
            {
                ref var sprite = ref animatedSprites.Get1(i);
                ref var animator = ref animatedSprites.Get2(i);

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
                    animator.Time += dt;
                    int shift = (int)(animator.Time / entry.FrameTime);
                    animator.Time -= shift * entry.FrameTime;
                    sprite.Tile = (sprite.Tile + shift) % entry.TilesCount;
                }
            }

            foreach (var i in animatedTiles)
            {
                ref var tiles = ref animatedTiles.Get1(i);
                var tileset = tiles.Tileset;

                foreach (var entry in tileset.Entries)
                {
                    entry.Time += dt;
                    int shift = (int)(entry.Time / entry.FrameTime);
                    entry.Time -= shift * entry.FrameTime;
                    entry.FrameTile = (entry.FrameTile + shift * entry.Size.Y) % entry.Quads.GetLength(1);
                }
            }
        }
    }
}
