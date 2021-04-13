using System.Collections.Generic;
using System.Text.Json.Serialization;
using Leopotam.Ecs;
using Lette.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Aether = tainicom.Aether.Physics2D;

namespace Lette.Components
{
    public interface IReplaceOnEntity
    {
        void Replace(EcsEntity entity);
    }

    public interface IReplaceOnEntity<T> : IReplaceOnEntity where T : struct
    {
        void IReplaceOnEntity.Replace(EcsEntity entity)
        {
            entity.Replace<T>((T)this);
        }
    }

    public interface IHandle
    {
        string Src { get; }

        [JsonIgnore]
        GenIdx Idx { get; set; }
    }

    public interface IValue<T>
    {
        T Value { get; set; }
    }

    public struct Sprite : IHandle, IReplaceOnEntity<Sprite>
    {
        public string Src { get; set; }
        public int Entry;
        public int Strip;
        public int Tile;

        [JsonIgnore]
        public GenIdx SheetIdx;
        public GenIdx Idx { get => SheetIdx; set => SheetIdx = value; }
    }

    public struct Tile : IReplaceOnEntity<Tile>
    {
        public int Entry;
        public Point Idx;
        public int Height;
    }

    public struct Tiles : IHandle, IReplaceOnEntity<Tiles>
    {
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

        public string Src { get; set; }
        public Tile[,,] Idx;

        [JsonIgnore]
        public GenIdx TilesetIdx;
        GenIdx IHandle.Idx { get => TilesetIdx; set => TilesetIdx = value; }
    }

    public struct Animator : IReplaceOnEntity<Animator>
    {
        public float Time;
        public Flags<AnimFlag> Flags;
    }

    public struct Pos : IReplaceOnEntity<Pos>, IValue<Vector2>
    {
        public Vector2 Value { get; set; }

        public static implicit operator Vector2(Pos pos) => pos.Value;
        public static implicit operator Pos(Vector2 pos) => new Pos { Value = pos };
    }

    public enum BodyShapeType
    {
        Circle
    }
    public struct BodyShape : IReplaceOnEntity<BodyShape>
    {
        public BodyShapeType Type;
        public float Radius;

        public static BodyShape Circle(float radius) => new BodyShape
        {
            Type = BodyShapeType.Circle,
            Radius = radius
        };
    }
    public struct Body : IEcsAutoReset<Body>, IReplaceOnEntity<Body>
    {
        public BodyShape Shape;

        [JsonIgnore]
        public Aether.Dynamics.Body? Physics;

        public void AutoReset(ref Body c)
        {
            c.Physics?.World.Remove(Physics);
            c.Physics = null;
        }
    }

    public struct StaticCollisions : IEcsAutoReset<Body>, IReplaceOnEntity<StaticCollisions>
    {
        public List<Vector2[]> Chains;

        [JsonIgnore]
        public Aether.Dynamics.Body? Physics;

        public void AutoReset(ref Body c)
        {
            c.Physics?.World.Remove(Physics);
            c.Physics = null;
        }
    }

    public struct Actor : IReplaceOnEntity<Actor>
    {
        public float Speed;
        public Flags<AnimFlag> Flags;
    }

    public struct KeyMap : IReplaceOnEntity<KeyMap>, IValue<Dictionary<Keys, (InputType, float)>>
    {
        public Dictionary<Keys, (InputType, float)> Value;

        Dictionary<Keys, (InputType, float)> IValue<Dictionary<Keys, (InputType, float)>>.Value
        {
            get => Value;
            set => Value = value;
        }
    }

    public struct Input : IReplaceOnEntity<Input>, IValue<EnumArray<InputType, float>>
    {
        public EnumArray<InputType, float> Value;

        EnumArray<InputType, float> IValue<EnumArray<InputType, float>>.Value
        {
            get => Value;
            set => Value = value;
        }
    }

    public struct Camera : IReplaceOnEntity<Camera>
    { }

    public struct Level : IReplaceOnEntity<Level>
    {
    }
}
