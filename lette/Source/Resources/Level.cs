using System.Collections.Generic;
using Lette.Components;

namespace Lette.Resources
{
    public class LevelDefinition
    {
        public string? Src;
        public Dictionary<string, EntityDefinition> Entities = new();
    }

    public class EntityDefinition : List<IReplaceOnEntity>
    {
        public int Mark;
    }
}
