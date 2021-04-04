using System.Collections.Generic;
using Lette.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lette.Resources
{
    public class TilesetEntry
    {
        public string Src;
        public Texture2D Texture;
        public float FPS;
        public float FrameTime;
        public float Time;
        public int FrameTile;
        public Point Size;
        public Rectangle[,] Quads;

        public Rectangle this[Point idx] => Quads[idx.X, idx.Y];
    }

    public class Tileset
    {
        public string Src;
        public Point Size;
        public TilesetEntry[] Entries;

        public void Init(Game game)
        {
            foreach (var entry in Entries)
            {
                entry.Texture = game.Content.Load<Texture2D>($"img/{ Src }/{ entry.Src }");
                entry.FrameTime = 1000f / entry.FPS;

                var size = entry.Texture.Size() / Size;
                entry.Quads = new Rectangle[size.X, size.Y];
                for (var x = 0; x < size.X; x++) {
                    for (var y = 0; y < size.Y; y++) {
                        entry.Quads[x, y] = new Rectangle(new Point(x, y) * Size, Size);
                    }
                }
            }
        }
    }
}
