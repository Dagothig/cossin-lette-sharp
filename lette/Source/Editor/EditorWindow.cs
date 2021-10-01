using System;
using Gtk;

namespace Lette.Editor
{
    public class EditorWindow : Window
    {
        public EditorWindow() : base("Éditeur lette")
        {
            DeleteEvent += (SendCompletedEventHandler, e) =>
            {
                Application.Quit();
            };

            AccelGroup accelGroup = new();
            AddAccelGroup(accelGroup);

            VBox vBox = new(false, 0);

            MenuBar menuBar = new();

            MenuItem fileItem = new("_File");

            Menu fileMenu = new();

            MenuItem newItem = new("_New");

            fileMenu.Append(newItem);

            MenuItem openItem = new("_Open");

            fileMenu.Append(openItem);

            MenuItem saveItem = new("_Save");

            fileMenu.Append(saveItem);

            fileMenu.Append(new SeparatorMenuItem());

            MenuItem exitItem = new("_Exit");
            exitItem.Activated += (sender, e) =>
            {
                Application.Quit();
            };
            exitItem.AddAccelerator("activate", accelGroup, new AccelKey(
                Gdk.Key.q, Gdk.ModifierType.ControlMask, AccelFlags.Visible));

            fileMenu.Append(exitItem);

            fileItem.Submenu = fileMenu;

            menuBar.Append(fileItem);

            vBox.PackStart(menuBar, false, false, 0);

            HBox hBox = new(false, 0);

            TreeView entities = new();

            entities.AppendColumn("Entity", new CellRendererText(), "text", 0);
            entities.AppendColumn("Name", new CellRendererText(), "text", 1);

            ListStore entitiesList = new(typeof(string), typeof(string));
            entitiesList.AppendValues("Le Entity", "Named");

            entities.Model = entitiesList;

            hBox.PackStart(entities, false, false, 0);

            vBox.PackStart(hBox, true, true, 0);

            Add(vBox);

            ShowAll();
        }
    }
}
