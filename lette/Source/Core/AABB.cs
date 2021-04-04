using System;
using Microsoft.Xna.Framework;
using static System.MathF;

namespace Lette.Core
{
    public struct AABB
    {
        public static readonly float BLEED_FACTOR = 0.001f;
        public static readonly Vector2 BLEED = Vector2.One * BLEED_FACTOR;

        public Vector2 Min, Max;

        public AABB(float mx, float my, float Mx, float My)
        {
            Min = new Vector2(mx, my);
            Max = new Vector2(Mx, My);
        }

        public bool Overlaps(AABB other) =>
            Min.X < other.Max.X && other.Min.X < Max.X &&
            Min.Y < other.Max.Y && other.Min.Y < Max.Y;

        public Rectangle Round()
        {
            var min = Min.FFloor();
            var max = Max.FCeil();
            return new Rectangle(min, max - min);
        }

        public AABB Bleed() =>
            new AABB
            {
                Min = Min - BLEED,
                Max = Max + BLEED
            };

        public AABB Intersect(AABB other) =>
            new AABB
            {
                Min = new Vector2(Max(Min.X, other.Min.X), Max(Min.Y, other.Min.Y)),
                Max = new Vector2(Min(Max.X, other.Max.X), Min(Max.Y, other.Max.Y))
            };

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

        public bool Similar(AABB other) =>
            (Min - other.Min).LengthSquared() < BLEED_FACTOR &&
            (Max - other.Max).LengthSquared() < BLEED_FACTOR;

        public static AABB operator *(AABB box, float multiplier) =>
            new AABB { Min = box.Min * multiplier, Max = box.Max * multiplier };

        public static AABB operator /(AABB box, float multiplier) =>
            new AABB { Min = box.Min / multiplier, Max = box.Max / multiplier };

        public static AABB operator /(AABB box, Vector2 vec) =>
            new AABB { Min = box.Min / vec, Max = box.Max / vec };

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
