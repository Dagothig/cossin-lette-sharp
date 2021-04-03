using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using static System.MathF;
using static System.Numerics.BitOperations;
namespace Lette.Core
{
    public static class Extensions
    {
        public static Point FFloor(this Vector2 value) =>
            new Point(
                (int)Floor(value.X),
                (int)Floor(value.Y));

        public static Point FCeil(this Vector2 value) =>
            new Point(
                (int)Ceiling(value.X),
                (int)Ceiling(value.Y));

        public static V GetOrCreate<K, V>(this Dictionary<K, V> dict, K key) where V : new()
        {
            if (!dict.TryGetValue(key, out V val))
            {
                val = new V();
                dict.Add(key, val);
            }
            return val;
        }

        public static IEnumerable<T> Stream<T>() where T : new()
        {
            while (true)
            {
                yield return new T();
            }
        }

        public static IEnumerable<T> Stream<T>(Func<T> fn)
        {
            while (true)
            {
                yield return fn();
            }
        }

        public static IEnumerable<T> TakeUntil<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (var t in source)
            {
                yield return t;
                if (predicate(t))
                    yield break;
            }
        }

        public static float Angle(this Flags<AnimFlag> flags) =>
            PI * (0.5f - Log2(flags.Backing & 0b11111111UL) / 4f);

        public static void SetAngle(ref this Flags<AnimFlag> flags, float angle)
        {
            flags.Backing &= ~0b11111111UL;
            flags[(int)Floor(10f - angle / PI * 4f + 0.5f) % 8] = true;
        }

        public static Vector2 V2(this float angle) =>
            new Vector2(Cos(angle), Sin(angle));
    }
}
