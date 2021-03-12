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
    }
}
