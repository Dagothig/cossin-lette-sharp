using System.Collections;
using System.Collections.Generic;

// TODO LOL
namespace Lette.Core
{
    public struct GenIdx
    {
        public int Index;
        public int Generation;

        public bool IsNull => Generation == 0;
    }

    public struct AllocatorEntry
    {
        public bool Alive;
        public int Generation;
    }

    public class GenIdxAllocator
    {
        public AllocatorEntry[] Entries;
        public Stack<int> Free = new Stack<int>();
        public int NextGeneration = 1;
        public int Count = 0;

        public GenIdxAllocator(int size = 10)
        {
            Entries = new AllocatorEntry[size];
        }

        public GenIdx Alloc()
        {
            if (!Free.TryPop(out var index))
                index = Count;
            var generation = NextGeneration++;
            Entries[index] = new AllocatorEntry
            {
                Alive = true,
                Generation = generation
            };
            return new GenIdx { Index = index, Generation = generation };
        }

        public bool Dealloc(GenIdx index)
        {
            var entry = Entries[index.Index];
            if (entry.Generation != index.Generation || !entry.Alive)
                return false;
            Free.Push(index.Index);
            entry.Alive = false;
            return true;
        }

        public bool Alive(GenIdx index)
        {
            var entry = Entries[index.Index];
            return entry.Alive && entry.Generation == index.Generation;
        }
    }

    public struct ArrEntry<T>
    {
        public T? Value;
        public int Generation;
    }

    public class GenArr<T> : IEnumerable<T>
    {
        public ArrEntry<T>[] Backing;
        public GenIdxAllocator Allocator;

        public GenArr(GenIdxAllocator allocator)
        {
            Backing = new ArrEntry<T>[allocator.Entries.Length];
            Allocator = allocator;
        }

        public T? this[GenIdx idx]
        {
            get => Allocator.Alive(idx) ? Backing[idx.Index].Value : default(T?);
            set
            {
                if (Allocator.Alive(idx))
                    Backing[idx.Index] = new ArrEntry<T>
                    {
                        Value = value,
                        Generation = idx.Generation
                    };
            }
        }

        public IEnumerable<T> Entries
        {
            get
            {
                for (var i = 0; i < Backing.Length; i++)
                {
                    var entry = Backing[i];
                    var idx = new GenIdx { Index = i, Generation = entry.Generation };
                    if (Allocator.Alive(idx) && entry.Value != null)
                        yield return entry.Value;
                }
            }
        }

        public IEnumerator<T> GetEnumerator() => Entries.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Entries.GetEnumerator();
    }
}
