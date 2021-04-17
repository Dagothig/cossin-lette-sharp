using System.Collections.Generic;
using Lette.Components;

namespace Lette.Resources
{
    public class LevelDefinition
    {
        public List<EntityDefinition> Entities = new();
    }

    public class EntityDefinition : List<IReplaceOnEntity>
    {}
}
