using Leopotam.Ecs;
using Lette.Core;
using Lette.Components;
using System.Linq;
using Lette.Resources;

namespace Lette.Systems
{
    public class AABBs : IEcsInitSystem, IEcsDestroySystem, IEcsRunSystem
    {
        public class SpatialEntriesListener : IEcsFilterListener
        {
            public SpatialEntriesListener(AABBs aabbs)
            {
                this.aabbs = aabbs;
            }

            internal AABBs aabbs;

            public void OnEntityAdded(in EcsEntity entity)
            {
                if (aabbs.spatialMap == null)
                    return;
                var entry = aabbs.spatialMap.Add(entity, entity.Get<AABB>());
                entity.Replace(entry);
            }

            public void OnEntityRemoved(in EcsEntity entity)
            {
                if (aabbs.spatialMap == null)
                    return;
                aabbs.spatialMap.Remove(entity.Get<SpatialEntry<EcsEntity>>());
                entity.Del<SpatialEntry<EcsEntity>>();
            }
        }

        EcsFilter<Pos, Sprite>? spriteBoxes = null;
        EcsFilter<AABB>? spatialEntries = null;
        SpatialEntriesListener? spatialEntriesListener = null;
        SpatialMap<EcsEntity>? spatialMap = null;
        GenArr<Sheet>? sheets = null;

        public void Init()
        {
            spatialEntriesListener = new SpatialEntriesListener(this);
            spatialEntries?.AddListener(spatialEntriesListener);
        }

        public void Destroy()
        {
            spatialEntries?.RemoveListener(spatialEntriesListener);
        }

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
                spriteBoxes.GetEntity(i).Replace(new AABB() { Min = min, Max = max });
            }

            if (spatialEntries != null && spatialMap != null) foreach (var i in spatialEntries)
            {
                ref var aabb = ref spatialEntries.Get1(i);
                ref var entry = ref spatialEntries.GetEntity(i).Get<SpatialEntry<EcsEntity>>();

                spatialMap.Update(ref entry, aabb);
            }
        }
    }
}
