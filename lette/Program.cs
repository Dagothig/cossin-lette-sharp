using System;
using Lette.Core;

namespace Lette
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new CossinLette())
            {
                game.Run();
            }
        }
    }
}
