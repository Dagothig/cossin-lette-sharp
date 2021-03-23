using Microsoft.Xna.Framework;
using Lette.Core;
using Microsoft.Xna.Framework.Graphics;

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
        public Vector2 Size;
        public Vector2 Decal;
        public int FPS;
        public float FrameTime;
        public int TilesCount;
        public Flags<AnimFlag> Flags;
        public Strip[] Strips;
    }

    public class Sheet
    {
        public Texture2D Texture;
        public SheetEntry[] Entries;
    }
}
