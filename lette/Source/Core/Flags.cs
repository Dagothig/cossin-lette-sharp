using System;

namespace Lette.Core
{
    public enum AnimFlag
    {
        DirB,
        DirBR,
        DirR,
        DirTR,
        DirT,
        DirTL,
        DirL,
        DirBL,
        Moving
    }

    public enum InputType
    {
        X,
        Y,
        Interaction
    }

    public struct Flags<T> where T : struct, IConvertible
    {
        public UInt64 Backing;

        public static Flags<T> New(params T[] init)
        {
            var flags = new Flags<T>();
            foreach (var t in init)
                flags[t] = true;
            return flags;
        }

        public bool this[int index]
        {
            get => (Backing & (1UL << index)) != 0;
            set {
                if (value) {
                    Backing |= (1UL << index);
                } else {
                    Backing &= ~(1UL << index);
                }
            }
        }

        public bool this[T t]
        {
            get => this[t.ToInt32(null)];
            set => this[t.ToInt32(null)] = value;
        }

        public bool Matches(Flags<T> toMatch) =>
            (Backing & toMatch.Backing) == toMatch.Backing;

        // True if this has false for the flags of the inverseMatch
        public bool DoesNotMatch(Flags<T> inverseMatch) =>
            (~Backing & inverseMatch.Backing) == inverseMatch.Backing;

        public static implicit operator Flags<T>(T single) =>
            Flags<T>.New(single);
    }
}
