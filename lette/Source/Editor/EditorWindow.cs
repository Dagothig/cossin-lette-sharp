using System;
using Gtk;
using Lette.Resources;

namespace Lette.Editor
{
    public class EditorWindow : Window
    {
        public HistoryStack History;
        public ListenableValue<LevelDefinition> LevelRef = new();
        public ListenableValue<EntityDefinition> EntityRef = new();

        public AccelGroup AccelGroup;

        public void Alert(string msg, Exception? exception = null)
        {
            var dialog = new MessageDialog(
                this,
                DialogFlags.Modal | DialogFlags.DestroyWithParent,
                MessageType.Error,
                ButtonsType.Ok,
                msg + (exception == null ? "" : "\n" + exception.Message));
            dialog.Title = "Éditeur Lette est triste";
            dialog.Run();
            dialog.Destroy();
        }

        public EditorWindow() : base("Éditeur Lette")
        {
            History = new(this);
            DeleteEvent += (SendCompletedEventHandler, e) => Application.Quit();

            LevelRef.OnChange += (level) => EntityRef.Value = null;

            AccelGroup = new();
            AddAccelGroup(AccelGroup);

            VBox vBox = new(false, 0);

            MenuBar menuBar = new();

            MenuItem fileItem = new("_File");
            fileItem.Submenu = new FileMenu(this);
            menuBar.Append(fileItem);

            MenuItem editItem = new("_Edit");
            editItem.Submenu = new HistoryMenu(this);
            menuBar.Append(editItem);

            vBox.PackStart(menuBar, false, false, 0);

            HBox hBox = new(false, 0);

            EntitiesWidget entities = new(this);
            hBox.PackStart(entities, false, false, 0);

            vBox.PackStart(hBox, true, true, 0);

            Add(vBox);

            ShowAll();

            LevelRef.Notify();
            EntityRef.Notify();
        }
    }
}
