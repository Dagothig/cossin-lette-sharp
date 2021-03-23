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

        public bool this[int index]
        {
            get => (Backing & (1UL << (index % 64))) != 0;
            set {
                if (value) {
                    Backing |= (1UL << (index % 64));
                } else {
                    Backing &= ~(1UL << (index % 64));
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
    }
}
