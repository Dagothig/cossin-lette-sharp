using Gtk;
using Leopotam.Ecs;

namespace Lette.Editor
{
    public static class EntitiesView
    {
        public static void New()
        {
            Application.Init();

            Window test = new("Cossin Lette");

            Label lbl = new();
            lbl.Text = "Buonjour";

            test.Add(lbl);

            test.ShowAll();

            Application.Run();
        }
    }
}
