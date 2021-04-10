using Lette.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;

namespace Lette.Resources
{
    public class TilesetEntry
    {
        public string? Src;
        public float FPS;
        public Point Size;

        [JsonIgnore]
        public Texture2D? Texture;

        [JsonIgnore]
        public float Time;

        [JsonIgnore]
        public float FrameTime;

        [JsonIgnore]
        public int FrameTile;

        [JsonIgnore]
        public Rectangle[,] Quads = new Rectangle[0,0];
    }

    public class Tileset
    {
        public string Src = string.Empty;
        public Point Size;
        public TilesetEntry[] Entries = Empty<TilesetEntry>.Array;
    }
}
