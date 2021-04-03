using Xunit;
using Lette.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Lette.Tests
{
    public class TestSpatialMap
    {
        public static void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            foreach (var entry in expected.Except(actual))
            {
                throw new System.Exception($"{entry} is missing from the set");
            }
            foreach (var entry in actual.Except(expected))
            {
                throw new System.Exception($"{entry} should not be in the set");
            }
        }

        [Fact]
        public void Indices()
        {
            Assert.Equal(
                new Rectangle(0, 0, 1, 1),
                new SpatialMap<int>(10).Indices(new AABB(0, 0, 1, 1)));

            Assert.Equal(
                new Rectangle(-1, -1, 2, 2),
                new SpatialMap<int>(10).Indices(new AABB(-1, -1, 2, 2)));

            Assert.Equal(
                new Rectangle(-1, 0, 4, 1),
                new SpatialMap<int>(1).Indices(new AABB(-0.5f, 0.5f, 3, 1)));
        }

        [Fact]
        public void Cells()
        {
            var map = new SpatialMap<int>(10);

            Assert.Empty(map.Cells(new AABB(0, 0, 1, 1)));

            // Realize the cells first so all the cell "seeding" occurs.
            var newCells = map.Cells(new AABB(0, 0, 1, 1), true).ToList();
            var existingCells = map.Cells(new AABB(0, 0, 1, 1)).ToList();
            SequenceEqual(newCells, existingCells);

            Assert.Equal(4, new SpatialMap<int>(10).Cells(new AABB(-1, -1, 2, 2), true).Count());

            Assert.Equal(4, new SpatialMap<int>(1).Cells(new AABB(-0.5f, 0.5f, 3, 1), true).Count());
        }

        [Fact]
        public void InsertAndGet()
        {
            var map = new SpatialMap<int>(1);
            var entry = map.Add(1, new AABB(0, 0, 1, 1));

            SequenceEqual(new int[] { 1 }, map.Region(new AABB(-1, -1, 2, 2)));

            map.Update(ref entry, new AABB(3, 3, 4, 4));

            SequenceEqual(new int[] {}, map.Region(new AABB(-1, -1, 2, 2)));
            SequenceEqual(new int[] { 1 }, map.Region(new AABB(2, 2, 3.5f, 3.5f)));

            map.Remove(entry);

            SequenceEqual(new int[] {}, map.Region(new AABB(-1, -1, 5, 5)));
        }

        [Fact]
        public void NegativeUpdate()
        {
            var map = new SpatialMap<int>(1);
            var entry = map.Add(1, new AABB(0.25f, 0.25f, 0.75f, 0.75f));
            var otherEntry = map.Add(2, new AABB(0.25f, 0.25f, 0.75f, 0.75f));

            SequenceEqual(new int[] { 1, 2 }, map.Region(new AABB(0, 0, 1, 1)));

            map.Update(ref entry, new AABB(-0.75f, 0.25f, -0.25f, 0.75f));

            SequenceEqual(new int[] { 2 }, map.Region(new AABB(0, 0, 1, 1)));
            SequenceEqual(new int[] { 1 }, map.Region(new AABB(-1, 0, 0, 1)));

            map.Update(ref entry, new AABB(-9.5f, 1f, -8.5f, 2f));

            SequenceEqual(new int[] { 1 }, map.Region(new AABB(-10, -8, 0, 3)));
        }
    }
}
