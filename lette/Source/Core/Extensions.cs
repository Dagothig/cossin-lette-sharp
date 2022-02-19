using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.MathF;
using static System.Numerics.BitOperations;
namespace Lette.Core
{
    public static class Enum<T>
    {
        public static string[] Names = Enum.GetNames(typeof(T));
        public static T[] Values = Enum.GetValues(typeof(T)).Of<T>();
        public static Dictionary<string, T> Map = new Dictionary<string, T>(
            Names.Zip(Values, (k, v) => new KeyValuePair<string, T>(k, v)));
    }

    public static class Empty<T>
    {
        public static T[] Array = new T[0];
    }

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

        public static V GetOrCreate<K, V>(this Dictionary<K, V> dict, K key)
        where K : notnull
        where V : new()
        {
            if (!dict.TryGetValue(key, out V? val))
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

        public static void Take<T>(this IEnumerable<T> source, out T one) =>
            one = source.Take(1).ToArray()[0];

        public static void Take<T>(this IEnumerable<T> source, out T x, out T y)
        {
            var arr = source.Take(2).ToArray();
            x = arr[0];
            y = arr[1];
        }

        public static void Take<T>(this IEnumerable<T> source, out T x, out T y, out T z)
        {
            var arr = source.Take(3).ToArray();
            x = arr[0];
            y = arr[1];
            z = arr[2];
        }

        public static void Take<T>(this IEnumerable<T> source, out T x, out T y, out T z, out T d)
        {
            var arr = source.Take(4).ToArray();
            x = arr[0];
            y = arr[1];
            z = arr[2];
            d = arr[3];
        }

        public static IEnumerable<R> Select2<T, R>(this IEnumerable<T> source, Func<T, T, R> select)
        {
            var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var x = enumerator.Current;
                if (!enumerator.MoveNext())
                    break;
                var y = enumerator.Current;
                yield return select(x, y);
            }
        }

        public static T[] Of<T>(this Array arr)
        {
            var res = new T[arr.Length];
            for (var i = 0; i < arr.Length; i++)
            {
                var val = arr.GetValue(i);
                if (val is T)
                    res[i] = (T)val;
            }
            return res;
        }

        public static ref Utf8JsonReader Advance(this ref Utf8JsonReader reader, out int num)
        {
            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new Exception();
            num = reader.GetInt32();
            return ref reader;
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

        public static Point Size(this Texture2D texture) =>
            new Point(texture.Width, texture.Height);

        public static Point Size<T>(this T[,] arr) =>
            new Point(arr.GetLength(0), arr.GetLength(1));

        public static string ToCamel(this string str) =>
            Char.ToLowerInvariant(str[0]) + str.Substring(1);

        public static string ToPascal(this string str) =>
            Char.ToUpperInvariant(str[0]) + str.Substring(1);

        public static void SetValue(this MemberInfo member, object property, object? value)
        {
            if (member.MemberType == MemberTypes.Property)
                ((PropertyInfo)member).SetValue(property, value, null);
            else if (member.MemberType == MemberTypes.Field)
                ((FieldInfo)member).SetValue(property, value);
            else
                throw new Exception("Property must be of type FieldInfo or PropertyInfo");
        }

        public static object? GetValue(this MemberInfo member, object property)
        {
            if (member.MemberType == MemberTypes.Property)
                return ((PropertyInfo)member).GetValue(property, null);
            else if (member.MemberType == MemberTypes.Field)
                return ((FieldInfo)member).GetValue(property);
            else
                throw new Exception("Property must be of type FieldInfo or PropertyInfo");
        }

        public static Type MemberType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException("MemberInfo must be of type FieldInfo, PropertyInfo", "member");
            }
        }

        public static void Set<T>(this List<T> list, T? value, Predicate<T> predicate)
        {
            var index = list.FindIndex(predicate);
            if (index >= 0)
                if (value == null)
                    list.RemoveAt(index);
                else
                    list[index] = value;
            else if (value != null)
                list.Add(value);
        }
    }
}
