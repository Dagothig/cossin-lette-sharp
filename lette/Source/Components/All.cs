using System.Collections.Generic;
using Leopotam.Ecs;
using Lette.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Aether = tainicom.Aether.Physics2D;

namespace Lette.Components
{
    public struct Sprite
    {
        public string Src;
        public GenIdx SheetIdx;
        public int Entry;
        public int Strip;
        public int Tile;
    }

    public struct Tile
    {
        public int Entry;
        public Point Idx;
        public int Height;
    }

    public struct Tiles
    {
        public string Src;
        public GenIdx TilesetIdx;
        public Tile[,,] Idx;

        public static Tile[,,] GenerateIdx(int w, int h, int d, params int[] exys)
        {
            var idx = new Tile[w, h, d];
            for (var x = 0; x < w; x++)
                for (var y = 0; y < h; y++)
                    for (var z = 0; z < d; z++)
                        {
                            var i = x + y * w + z * w * h;
                            idx[x, y, z] = new Tile()
                            {
                                Entry = exys[i * 4],
                                Idx = new Point(exys[i * 4 + 1], exys[i * 4 + 2]),
                                Height = exys[i * 4 + 3]
                            };
                        }
            return idx;
        }
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

        public static BodyShape Circle(float radius) => new BodyShape
        {
            Type = BodyShapeType.Circle,
            Radius = radius
        };
    }
    public struct Body : IEcsAutoReset<Body>
    {
        public BodyShape Shape;
        public Aether.Dynamics.Body? Physics;

        public void AutoReset(ref Body c)
        {
            c.Physics?.World.Remove(Physics);
            c.Physics = null;
        }
    }

    public struct StaticCollisions : IEcsAutoReset<Body>
    {
        public List<Vector2[]> Chains;
        public Aether.Dynamics.Body? Physics;

        public void AutoReset(ref Body c)
        {
            c.Physics?.World.Remove(Physics);
            c.Physics = null;
        }
    }

    public struct Actor
    {
        public float Speed;
        public Flags<AnimFlag> Flags;
    }

    public struct KeyMap
    {
        public Dictionary<Keys, (InputType, float)> Value;
    }

    public struct Input
    {
        public EnumArray<InputType, float> Value;
    }

    public struct Camera
    { }

    public struct Owner
    {
        public EcsEntity Value;
    }
}
