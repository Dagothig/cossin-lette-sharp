using System;
using System.Linq;
using Lette.Core;
using Lette.States;

namespace Lette
{
    public static class Program
    {
        public const string HELP = @"
            Usage: cossin [OPTION]...
            {--state|-s} {editor|game}
            {--help|-h}";

        [STAThread]
        static int Main(string[] args)
        {
            IState? initState = null;

            var enumerator = args.GetEnumerator();
            while (enumerator.MoveNext())
            {
                switch (enumerator.Current)
                {
                    case "--state":
                    case "-s":
                        if (!enumerator.MoveNext())
                        {
                            Console.WriteLine("Option '--state' expected argument 'editor|game'");
                            return 1;
                        }
                        switch (enumerator.Current)
                        {
                            case "editor":
                                initState = new EditorState();
                                break;
                            case "game":
                                initState = new GameState();
                                break;
                            default:
                                Console.WriteLine($"Option '--state' expected argument 'editor|game', but got { enumerator.Current }");
                                return 1;
                        }
                        break;
                    case "--help":
                    case "-h":
                        Console.WriteLine(HELP);
                        return 0;
                    default:
                        Console.WriteLine($"Unrecognized option '{ enumerator.Current }'");
                        Console.WriteLine(HELP);
                        return 1;
                }
            }

            using (var game = new CossinLette(initState))
                game.Run();
            return 0;
        }
    }
}
