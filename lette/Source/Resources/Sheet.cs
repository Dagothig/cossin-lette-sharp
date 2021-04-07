using Microsoft.Xna.Framework;
using Lette.Core;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Text.Json.Serialization;
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
        public Vector2 Size;
        public Vector2 Decal;
        public float FPS;
        public Flags<AnimFlag> Flags;
        public (Flags<AnimFlag>, Vector2)[][] StripsVariants;

        public string StripsVariantsPreset
        {
            get => Sheet.StripsVariantsPresets.FirstOrDefault(e =>
                e.Value == StripsVariants).Key;
            set => StripsVariants = Sheet.StripsVariantsPresets[value];
        }

        [JsonIgnore]
        public Texture2D Texture;

        [JsonIgnore]
        public float FrameTime;

        [JsonIgnore]
        public int TilesCount;

        [JsonIgnore]
        public Strip[] Strips;
    }

    public class Sheet
    {
        public static Dictionary<string, (Flags<AnimFlag>, Vector2)[][]> StripsVariantsPresets =
            new Dictionary<string, (Flags<AnimFlag>, Vector2)[][]>
            {
                {
                    "FlipDirStrips",
                    new (Flags<AnimFlag>, Vector2)[][]
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
                    }
                }
            };

        public string Src;
        public SheetEntry[] Entries;
    }
}
