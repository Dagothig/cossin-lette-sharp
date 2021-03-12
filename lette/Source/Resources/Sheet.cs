using Microsoft.Xna.Framework;
using Lette.Core;

namespace Lette.Resources
{
    public class Tile
    {
        public Vector2 Position;
        public Vector2 Size;
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
        public SheetEntry[] Entries;
    }
}
