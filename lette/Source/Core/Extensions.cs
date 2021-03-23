using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Lette.Core
{
    public static class Extensions
    {
        public static Point FFloor(this Vector2 value) =>
            new Point(
                (int)Math.Floor(value.X),
                (int)Math.Floor(value.Y));

        public static Point FCeil(this Vector2 value) =>
            new Point(
                (int)Math.Ceiling(value.X),
                (int)Math.Ceiling(value.Y));

        public static V GetOrCreate<K, V>(this Dictionary<K, V> dict, K key) where V : new()
        {
            V val;
            if (!dict.TryGetValue(key, out val))
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
            2f * MathF.PI * (flags.Backing & 11111111UL) / 8f;

        public static void SetAngle(this Flags<AnimFlag> flags, float angle)
        {
            flags.Backing &= ~(11111111UL);
            flags[(int)MathF.Floor(angle / MathF.PI * 4f + 0.5f)] = true;
        }

        public static Vector2 V2(this float angle) =>
            new Vector2(
                MathF.Cos(angle),
                MathF.Sin(angle));
    }
}
