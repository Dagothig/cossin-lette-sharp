using Leopotam.Ecs;
using Lette.Core;
using Lette.Components;

namespace Lette.Systems
{
    public class AABBs : IEcsInitSystem, IEcsDestroySystem, IEcsRunSystem
    {
        public class SpatialEntriesListener : IEcsFilterListener
        {
            internal AABBs aabbs;

            public void OnEntityAdded(in EcsEntity entity)
            {
                var entry = aabbs.spatialMap.Add(entity, entity.Get<AABB>());
                entity.Replace(entry);
            }

            public void OnEntityRemoved(in EcsEntity entity)
            {
                aabbs.spatialMap.Remove(entity.Get<SpatialEntry<EcsEntity>>());
                entity.Del<SpatialEntry<EcsEntity>>();
            }
        }

        EcsFilter<Pos, Sprite> spriteBoxes = null;
        EcsFilter<AABB> spatialEntries = null;
        SpatialEntriesListener spatialEntriesListener = null;
        SpatialMap<EcsEntity> spatialMap = null;

        public void Init()
        {
            spatialEntriesListener = new SpatialEntriesListener() { aabbs = this };
            spatialEntries.AddListener(spatialEntriesListener);
        }

        public void Destroy()
        {
            spatialEntries.RemoveListener(spatialEntriesListener);
        }

        public void Run()
        {
            foreach (var i in spriteBoxes)
            {
                ref var pos = ref spriteBoxes.Get1(i);
                ref var sprite = ref spriteBoxes.Get2(i);

                var sheetEntry = sprite.Sheet.Entries[sprite.Entry];
                var min = pos - sheetEntry.Decal;
                var max = min + sheetEntry.Size;
                spriteBoxes.GetEntity(i).Replace(new AABB() { Min = min, Max = max });
            }

            foreach (var i in spatialEntries)
            {
                ref var aabb = ref spatialEntries.Get1(i);
                ref var entry = ref spatialEntries.GetEntity(i).Get<SpatialEntry<EcsEntity>>();

                spatialMap.Update(ref entry, aabb);
            }
        }
    }
}
