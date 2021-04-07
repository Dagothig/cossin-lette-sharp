using System.Collections.Generic;

// TODO LOL
namespace Lette.Core
{
    public struct GenerationalIndex
    {
        public int Index;
        public int Generation;
    }

    public struct AllocatorEntry
    {
        public bool Alive;
        public int Generation;
    }

    public class GenerationalIndexAllocator
    {
        public AllocatorEntry[] Entries;
        public Stack<int> Free = new Stack<int>();
        public int NextGeneration = 0;
        public int Count = 0;

        public GenerationalIndexAllocator(int size)
        {
            Entries = new AllocatorEntry[size];
        }

        public GenerationalIndex Alloc()
        {
            if (!Free.TryPop(out var index))
                index = Count;
            var generation = NextGeneration++;
            Entries[index] = new AllocatorEntry
            {
                Alive = true,
                Generation = generation
            };
            return new GenerationalIndex { Index = index, Generation = generation };
        }

        public bool Dealloc(GenerationalIndex index)
        {
            var entry = Entries[index.Index];
            if (entry.Generation != index.Generation || !entry.Alive)
                return false;
            Free.Push(index.Index);
            entry.Alive = false;
            return true;
        }

        public bool Alive(GenerationalIndex index)
        {
            var entry = Entries[index.Index];
            return entry.Alive && entry.Generation == index.Generation;
        }
    }

    public struct ArrayEntry<T> where T : struct
    {
        public T Value;
        public int Generation;
    }

    /*public struct GenerationalArray<T> where T : struct
    {
        public ArrayEntry<T>[] Backing;

        public GenerationalArray()
        {
        }

        public ref T this[GenerationalIndex index]
        {
            get
            {
                ref var entry = ref Backing[index.Index];
                return entry.Value;
                return entry.Generation == index.Generation ? entry.Value : (T?)null;
            }
            set
            {
                Backing[index.Index] = new ArrayEntry<T>
                {
                    Value = value,
                    Generation = index.Generation
                };
            }
        }
    }*/
}
