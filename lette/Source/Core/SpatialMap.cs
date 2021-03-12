using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Lette.Core
{
    public struct SpatialEntry<T>
    {
        public AABB Bounds;
        public Vector2 Pos;
        public T Value;
    }

    public class SpatialMap<T>
    {
        public float CellSize;
        public Dictionary<long, List<SpatialEntry<T>>> Backing = new Dictionary<long, List<SpatialEntry<T>>>();

        public SpatialMap(float cellSize)
        {
            CellSize = cellSize;
        }

        public Rectangle Indices(AABB bounds) =>
            (bounds / CellSize).Round();

        public long Key(int x, int y) =>
            (long)x | ((long)y << 32);


        public IEnumerable<List<SpatialEntry<T>>> Cells(AABB bounds, bool createCells = false)
        {
            var indices = Indices(bounds);
            for (var x = (int)indices.X; x < indices.Width; x++)
                for (var y = (int)indices.Y; y < indices.Height; y++)
                {
                    long key = Key(x, y);
                    if (Backing.ContainsKey(key))
                        yield return Backing[key];
                    else if (createCells)
                        yield return Backing[key] = new List<SpatialEntry<T>>();
                }
        }

        public IEnumerable<T> Region(AABB bounds, bool loose = false) =>
            Cells(bounds)
            .SelectMany(cell => loose ? cell : cell.Where(entry => entry.Bounds.Overlaps(bounds)))
            .Select(entry => entry.Value)
            .Distinct();

        public SpatialEntry<T> Insert(T value, Vector2 pos, AABB bounds)
        {
            var entry = new SpatialEntry<T> { Value = value, Pos = pos, Bounds = bounds };
            foreach (var cell in Cells(bounds, true))
                cell.Add(entry);
            return entry;
        }

        public void Remove(SpatialEntry<T> entry)
        {
            foreach (var cell in Cells(entry.Bounds, false))
                cell.Remove(entry);
        }

        public SpatialEntry<T> Update(SpatialEntry<T> entry, Vector2 pos, AABB bounds)
        {
            if (entry.Pos == pos && entry.Bounds == bounds)
                return entry;
            var newEntry = new SpatialEntry<T> { Value = entry.Value, Pos = pos, Bounds = bounds };

            var o = Indices(entry.Bounds);
            var n = Indices(bounds);

            for (var x = o.X; x < o.Width; x++)
                for (var y = o.Y; y < o.Height; y++)
                    if (x < n.X || n.Width >= x || y < n.Y || n.Height >= y)
                        Backing[Key(x, y)]?.Remove(entry);

            for (var x = n.X; x < n.Width; x++)
                for (var y = n.Y; y < n.Height; y++)
                    if (x < o.X || o.Width >= x || y < o.Y || o.Height >= y)
                        Backing.GetOrCreate(Key(x, y)).Add(entry);

            return newEntry;
        }
    }
}
