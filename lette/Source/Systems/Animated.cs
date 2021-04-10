using System;
using Lette.Components;
using Leopotam.Ecs;
using Lette.Resources;
using Lette.Core;

namespace Lette.Systems
{
    public class Animated : IEcsRunSystem
    {
        EcsFilter<Sprite, Animator>? animatedSprites = null;
        GenArr<Sheet>? sheets = null;
        GenArr<Tileset>? tilesets = null;
        TimeSpan step = TimeSpan.MinValue;

        public void Run()
        {
            var dt = (float)step.TotalMilliseconds;

            if (animatedSprites != null) foreach (var i in animatedSprites)
            {
                ref var sprite = ref animatedSprites.Get1(i);
                ref var animator = ref animatedSprites.Get2(i);

                var sheet = sheets?[sprite.SheetIdx];
                if (sheet == null)
                    continue;
                var flags = animator.Flags;

                // TODO - This is bad

                var entryIdx = 0;
                SheetEntry? entry = null;
                for (var ei = 0; ei < sheet.Entries.Length; ei++)
                {
                    entry = sheet.Entries[ei];
                    if (flags.Matches(sheet.Entries[ei].Flags))
                    {
                        entryIdx = ei;
                        break;
                    }
                }

                var stripIdx = 0;
                for (var si = 0; si < entry?.Strips?.Length; si++)
                    if (flags.Matches(entry.Strips[si].Flags))
                    {
                        stripIdx = si;
                        break;
                    }

                if (sprite.Entry != entryIdx || sprite.Strip != stripIdx)
                {
                    animator.Time = 0;
                    sprite.Tile = 0;
                    sprite.Entry = entryIdx;
                    sprite.Strip = stripIdx;
                }
                else
                {
                    if (entry != null && entry.FPS > 0) {
                        animator.Time += dt;
                        int shift = (int)(animator.Time / entry.FrameTime);
                        animator.Time -= shift * entry.FrameTime;
                        sprite.Tile = (sprite.Tile + shift) % entry.TilesCount;
                    }
                }
            }

            if (tilesets != null) foreach (var tileset in tilesets)
            {
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
