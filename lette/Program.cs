using System;
using Lette.Core;

namespace Lette
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            Flags<AnimFlag> flags = new Flags<AnimFlag>();
            flags[AnimFlag.DirL] = true;
            if (!flags[AnimFlag.DirL] || flags[AnimFlag.Moving])
                throw new Exception("BANANA");
            flags[AnimFlag.DirR] = true;
            if (!flags[AnimFlag.DirL] || flags[AnimFlag.Moving] || !flags[AnimFlag.DirR])
                throw new Exception("BANANA");
            if (!flags.Matches(Flags<AnimFlag>.New(AnimFlag.DirL, AnimFlag.DirR)))
                throw new Exception("BANANA");
            ulong test = 1 | 1 << 16;
            test |= 1 << 1;
            test &= ~(11UL);
            if (test != 1 << 16)
                throw new Exception("BANANA");
            using (var game = new CossinLette())
            {
                game.Run();
            }
        }
    }
}
