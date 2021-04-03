using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Lette.Core
{
    public struct SpatialEntry<T> : IEquatable<SpatialEntry<T>>
    {
        public AABB Bounds;
        public T Value;

        public bool Equals(SpatialEntry<T> other) =>
            Value.Equals(other.Value);

        public override string ToString() =>
            $"[{ Bounds }:{ Value }]";
    }

    public class SpatialMap<T> : IEqualityComparer<T> where T : IEquatable<T>
    {
        public float CellSize;
        public Dictionary<Point, List<SpatialEntry<T>>> Backing = new Dictionary<Point, List<SpatialEntry<T>>>();

        public SpatialMap(float cellSize)
        {
            CellSize = cellSize;
        }

        public bool Equals(T x, T y) => x.Equals(y);
        public int GetHashCode(T obj) => obj.GetHashCode();

        public Rectangle Indices(AABB bounds) =>
            (bounds / CellSize).Round();

        public IEnumerable<List<SpatialEntry<T>>> Cells(AABB bounds, bool createCells = false)
        {
            var indices = Indices(bounds);
            for (var x = 0; x < indices.Width; x++)
                for (var y = 0; y < indices.Height; y++)
                {
                    var pt = new Point(indices.X + x, indices.Y + y);
                    if (Backing.ContainsKey(pt))
                        yield return Backing[pt];
                    else if (createCells)
                        yield return Backing[pt] = new List<SpatialEntry<T>>();
                }
        }

        public IEnumerable<T> Region(AABB bounds, bool loose = false) =>
            Cells(bounds)
            .SelectMany(cell => loose ? cell : cell.Where(entry => entry.Bounds.Overlaps(bounds)))
            .Select(entry => entry.Value)
            .Distinct(this);

        public SpatialEntry<T> Add(T value, AABB bounds)
        {
            var entry = new SpatialEntry<T> { Value = value, Bounds = bounds };
            foreach (var cell in Cells(bounds, true))
                cell.Add(entry);
            return entry;
        }

        public void Remove(SpatialEntry<T> entry)
        {
            var indices = Indices(entry.Bounds);
            for (var x = 0; x < indices.Width; x++)
                for (var y = 0; y < indices.Height; y++)
                {
                    var pt = new Point(indices.X + x, indices.Y + y);
                    if (Backing.TryGetValue(pt, out var cell))
                    {
                        cell.Remove(entry);
                        if (cell.Count == 0)
                            Backing.Remove(pt);
                    }
                }
        }

        public void Update(ref SpatialEntry<T> entry, AABB bounds)
        {
            if (entry.Bounds.Similar(bounds))
                return;
            var o = Indices(entry.Bounds);
            var n = Indices(bounds);

            entry.Bounds = bounds;

            for (var x = o.X; x < o.Right; x++)
                for (var y = o.Y; y < o.Bottom; y++)
                    if (x < n.X || x >= n.Right || y < n.Y || y >= n.Bottom)
                    {
                        var pt = new Point(x, y);
                        if (Backing.TryGetValue(pt, out var list))
                        {
                            list.Remove(entry);
                            if (list.Count == 0)
                                Backing.Remove(pt);
                        }
                    }

            for (var x = n.X; x < n.Right; x++)
                for (var y = n.Y; y < n.Bottom; y++)
                    {
                        var list = Backing.GetOrCreate(new Point(x, y));
                        for (var i = 0; i < list.Count; i++)
                            if (list[i].Value.Equals(entry.Value))
                            {
                                list[i] = entry;
                                break;
                            }
                            list.Add(entry);
                    }

        }
    }
}
