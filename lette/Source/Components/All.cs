using Lette.Core;
using Lette.Resources;

namespace Lette.Components
{
    public struct Sprite
    {
        public string Src;
        public int Entry;
        public int Strip;
        public int Tile;
        public Sheet Sheet;
    }

    public struct Animator
    {
        public float Time;
        public Flags<AnimFlag> Flags;
    }
}
