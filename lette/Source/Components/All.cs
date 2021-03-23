using Lette.Core;
using Lette.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Aether = tainicom.Aether.Physics2D;
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

    public struct Pos
    {
        public Vector2 Value;

        public static implicit operator Vector2(Pos pos) => pos.Value;
        public static implicit operator Pos(Vector2 pos) => new Pos { Value = pos };
    }

    public enum BodyShapeType
    {
        Circle
    }
    public struct BodyShape
    {
        public BodyShapeType Type;
        public float Radius;
    }
    public struct Body
    {
        public BodyShape Shape;
        public Aether.Dynamics.Body Physics;
    }

    public struct Actor
    {
        public float Speed;
        public Flags<AnimFlag> Flags;
    }

    public struct InputEntry
    {
        public string Type;
        public float Value;
    }

    public struct KeyMap
    {
        public EnumArray<Keys, (InputType, float)> Value;
    }

    public struct Input
    {
        public EnumArray<InputType, float> Value;
    }

    public struct Camera
    { }
}
