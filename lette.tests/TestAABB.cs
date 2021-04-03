using Xunit;
using Lette.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Lette.Tests
{
    public class TestAABB
    {
        [Fact]
        public void Overlaps()
        {
            Assert.True(new AABB(0, 0, 1, 1).Overlaps(new AABB(-1, -1, 2, 2)));
            Assert.False(new AABB(0, 0, 1, 1).Overlaps(new AABB(1, 0, 1, 1)));
            Assert.True(new AABB(0, 0, 1, 1).Overlaps(new AABB(0.99f, 0, 1, 1)));
        }
    }
}
