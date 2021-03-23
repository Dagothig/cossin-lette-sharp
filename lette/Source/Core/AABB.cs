using System;
using Microsoft.Xna.Framework;

namespace Lette.Core
{
    public struct AABB
    {
        public Vector2 Min, Max;

        public bool Overlaps(AABB other) =>
            Min.X < other.Max.X && other.Min.X < Max.X &&
            Min.Y < other.Max.Y && other.Min.Y < Max.Y;

        public Rectangle Round() =>
            new Rectangle(Min.FFloor(), (Max - Min).FCeil());

        public override bool Equals(object other) =>
            other is AABB && this == (AABB)other;

        public override int GetHashCode() =>
            System.HashCode.Combine(Min, Max);

        public override string ToString() =>
            $"[{ Min }:{ Max }]";

        public static bool operator ==(AABB lhs, AABB rhs) =>
            lhs.Min == rhs.Min && lhs.Max == rhs.Max;

        public static bool operator !=(AABB lhs, AABB rhs) =>
            !(lhs == rhs);

        public static AABB operator *(AABB box, float multiplier) =>
            new AABB { Min = box.Min * multiplier, Max = box.Max * multiplier };

        public static AABB operator /(AABB box, float multiplier) =>
            new AABB { Min = box.Min / multiplier, Max = box.Max / multiplier };

        public static AABB operator +(AABB box, Vector2 vec) =>
            new AABB { Min = box.Min + vec, Max = box.Max + vec };

        public static AABB operator -(AABB box, Vector2 vec) =>
            new AABB { Min = box.Min - vec, Max = box.Max - vec };

        public static implicit operator AABB(Rectangle rect)
        {
            var min = rect.Location.ToVector2();
            var max = min + rect.Size.ToVector2();
            return new AABB { Min = min, Max = max };
        }
    }
}
