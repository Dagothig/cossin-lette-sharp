using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;

namespace Lette.Resources
{
    public class TilesetEntry
    {
        public string Src;
        public float FPS;
        public Point Size;

        [JsonIgnore]
        public Texture2D Texture;

        [JsonIgnore]
        public float Time;

        [JsonIgnore]
        public float FrameTime;

        [JsonIgnore]
        public int FrameTile;

        [JsonIgnore]
        public Rectangle[,] Quads;

        [JsonIgnore]
        public Rectangle this[Point idx] => Quads[idx.X, idx.Y];
    }

    public class Tileset
    {
        public string Src;
        public Point Size;
        public TilesetEntry[] Entries;
    }
}
