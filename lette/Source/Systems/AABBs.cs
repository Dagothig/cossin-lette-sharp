using Leopotam.Ecs;
using Lette.Core;
using Lette.Components;
using Lette.Resources;

namespace Lette.Systems
{
    public class AABBs : IEcsRunSystem
    {
        EcsFilter<Pos, Sprite>? spriteBoxes = null;
        GenArr<Sheet>? sheets = null;

        public void Run()
        {
            if (spriteBoxes != null) foreach (var i in spriteBoxes)
            {
                ref var pos = ref spriteBoxes.Get1(i);
                ref var sprite = ref spriteBoxes.Get2(i);

                var sheet = sheets?[sprite.SheetIdx];
                var sheetEntry = sheet?.Entries[sprite.Entry % sheet.Entries.Length];
                if (sheetEntry == null)
                    continue;
                var min = pos.Value * Constants.PIXELS_PER_METER - sheetEntry.Decal;
                var max = min + sheetEntry.Size;
                ref var entity = ref spriteBoxes.GetEntity(i);
                entity.Replace(new AABB() { Min = min, Max = max });
            }
        }
    }
}
