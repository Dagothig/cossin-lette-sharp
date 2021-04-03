using Microsoft.Xna.Framework;
using Lette.Core;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Lette.Resources
{
    public class Tile
    {
        public Rectangle Quad;
        public Vector2 Scale;
    }

    public class Strip
    {
        public Flags<AnimFlag> Flags;
        public Tile[] Tiles;
    }

    public class SheetEntry
    {
        public string Src;
        public Texture2D Texture;
        public Vector2 Size;
        public Vector2 Decal;
        public float FPS;
        public float FrameTime;
        public int TilesCount;
        public Flags<AnimFlag> Flags;
        public (Flags<AnimFlag>, Vector2)[][] StripsVariants;
        public Strip[] Strips;
    }

    public class Sheet
    {
        public static (Flags<AnimFlag>, Vector2)[][] FlipDirStripsVariants = new (Flags<AnimFlag>, Vector2)[][]
        {
            new (Flags<AnimFlag>, Vector2)[]
            {
                (AnimFlag.DirB, Vector2.One)
            },
            new (Flags<AnimFlag>, Vector2)[]
            {
                (AnimFlag.DirBR, Vector2.One),
                (AnimFlag.DirBL, new Vector2(-1, 1))
            },
            new (Flags<AnimFlag>, Vector2)[]
            {
                (AnimFlag.DirR, Vector2.One),
                (AnimFlag.DirL, new Vector2(-1, 1))
            },
            new (Flags<AnimFlag>, Vector2)[]
            {
                (AnimFlag.DirTR, Vector2.One),
                (AnimFlag.DirTL, new Vector2(-1, 1))
            },
            new (Flags<AnimFlag>, Vector2)[]
            {
                (AnimFlag.DirT, Vector2.One)
            },
        };

        public string Src;
        public SheetEntry[] Entries;

        public void Init(Game game)
        {
            foreach (var entry in Entries)
            {
                entry.Texture = game.Content.Load<Texture2D>($"img/{ Src }/{ entry.Src }");
                entry.FrameTime = 1000f / entry.FPS;
                entry.TilesCount = (int)(entry.Texture.Width / entry.Size.X);
                var ptSize = entry.Size.ToPoint();
                entry.Strips = entry.StripsVariants
                    .SelectMany((variants, j) => variants
                        .Select(variant => new Strip
                        {
                            Flags = variant.Item1,
                            Tiles = Enumerable
                                .Range(0, entry.TilesCount)
                                .Select(i => new Tile
                                {
                                    Quad = new Rectangle(new Point(i, j) * ptSize, ptSize),
                                    Scale = variant.Item2
                                })
                                .ToArray()
                        }))
                    .ToArray();
            }
        }
    }
}
