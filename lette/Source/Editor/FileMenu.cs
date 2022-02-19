using System;
using System.IO;
using System.Text.Json;
using Gtk;
using Lette.Core.JsonSerialization;
using Lette.Resources;

namespace Lette.Editor
{
    public class FileMenu : Menu
    {
        EditorWindow window;
        FileFilter levelFilter;

        public FileMenu(EditorWindow parent)
        {
            this.window = parent;

            levelFilter = new();
            levelFilter.AddMimeType("application/json");

            MenuItem newItem = new("_New Level");
            newItem.Activated += (sender, e) => NewLevel();
            newItem.AddAccelerator("activate", parent.AccelGroup, new AccelKey(
                Gdk.Key.n, Gdk.ModifierType.ControlMask, AccelFlags.Visible));
            Append(newItem);

            MenuItem openItem = new("_Open Level");
            openItem.Activated += (sender, e) => OpenLevel();
            openItem.AddAccelerator("activate", parent.AccelGroup, new AccelKey(
                Gdk.Key.o, Gdk.ModifierType.ControlMask, AccelFlags.Visible));
            Append(openItem);

            MenuItem saveItem = new("_Save");
            saveItem.Activated += (sender, e) => SaveLevel(false);
            saveItem.AddAccelerator("activate", parent.AccelGroup, new AccelKey(
                Gdk.Key.s, Gdk.ModifierType.ControlMask, AccelFlags.Visible));
            parent.LevelRef.OnChange += level => saveItem.Sensitive = level != null;
            Append(saveItem);

            MenuItem saveAsItem = new("Save _As");
            saveAsItem.Activated += (sender, e) => SaveLevel(true);
            saveAsItem.AddAccelerator("activate", parent.AccelGroup, new AccelKey(
                Gdk.Key.s, Gdk.ModifierType.ControlMask | Gdk.ModifierType.ShiftMask, AccelFlags.Visible));
            parent.LevelRef.OnChange += level => saveAsItem.Sensitive = level != null;
            Append(saveAsItem);

            Append(new SeparatorMenuItem());

            MenuItem exitItem = new("_Exit");
            exitItem.Activated += (sender, e) => Application.Quit();
            exitItem.AddAccelerator("activate", parent.AccelGroup, new AccelKey(
                Gdk.Key.q, Gdk.ModifierType.ControlMask, AccelFlags.Visible));

            Append(exitItem);
        }

        public void NewLevel()
        {
            window.LevelRef.Value = new();
            window.History.Clear();
        }

        public void OpenLevel()
        {
            var dialog = new FileChooserDialog(
                "Open Level",
                window,
                FileChooserAction.Open,
                Stock.Cancel, ResponseType.Cancel,
                Stock.Open, ResponseType.Accept);
            dialog.Filter = levelFilter;
            dialog.SetCurrentFolder(System.IO.Directory.GetCurrentDirectory());
            if ((ResponseType)dialog.Run() != ResponseType.Accept)
                return;
            var filename = dialog.Filename;
            dialog.Destroy();

            try
            {
                var json = File.ReadAllText(filename);
                var level = JsonSerializer.Deserialize<LevelDefinition>(json, JsonSerialization.Options);
                level!.Src = filename;
                window.LevelRef.Value = level;
                window.History.Clear();
            }
            catch (Exception exception)
            {
                window.Alert("Erreur lors de la lecture du niveau", exception);
            }
        }

        public void SaveLevel(bool alwaysAskForFile)
        {
            var filename = window.LevelRef.Value!.Src;
            if (filename == null || alwaysAskForFile)
            {
                var dialog = new FileChooserDialog(
                    "Save Level",
                    window,
                    FileChooserAction.Save,
                    Stock.Cancel, ResponseType.Cancel,
                    Stock.Save, ResponseType.Accept);
                dialog.Filter = levelFilter;
                if ((ResponseType)dialog.Run() != ResponseType.Accept)
                    return;
                filename = dialog.Filename;
                dialog.Destroy();
            }
            try
            {
                var json = JsonSerializer.Serialize(window.LevelRef.Value, JsonSerialization.Options);
                File.WriteAllText(json, json);
                window.LevelRef.Value!.Src = filename;   
            }
            catch (Exception exception) 
            {
                window.Alert("Erreur lors de l'enregistrement du niveau", exception);
            }
        }
    }
}
