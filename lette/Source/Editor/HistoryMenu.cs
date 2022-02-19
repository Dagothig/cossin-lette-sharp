using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;

namespace Lette.Editor
{
    public class HistoryMenu : Menu
    {
        EditorWindow window;
        List<(MenuItem, ICommand)> historyItems = new();
        Menu historyMenu;

        public HistoryMenu(EditorWindow parent) 
        {
            this.window = parent;

            MenuItem undo = new("_Undo");
            undo.Activated += (sender, e) => window.History.Undo();
            undo.Sensitive = false;
            undo.AddAccelerator("activate", parent.AccelGroup, new AccelKey(
                Gdk.Key.z, Gdk.ModifierType.ControlMask, AccelFlags.Visible));
            Append(undo);

            MenuItem redo = new("_Redo");
            redo.Activated += (sender, e) => window.History.Redo();
            redo.Sensitive = false;
            redo.AddAccelerator("activate", parent.AccelGroup, new AccelKey(
                Gdk.Key.z, Gdk.ModifierType.ControlMask | Gdk.ModifierType.ShiftMask, AccelFlags.Visible));
            redo.AddAccelerator("activate", parent.AccelGroup, new AccelKey(
                Gdk.Key.y, Gdk.ModifierType.ControlMask, AccelFlags.Visible));
            Append(redo);

            Append(new SeparatorMenuItem());

            MenuItem historyMenuItem = new("_History");
            historyMenuItem.Sensitive = false;
            historyMenuItem.Submenu = historyMenu = new();
            Append(historyMenuItem);
            
            parent.History.OnChange += (stack, index) =>
            {
                undo.Sensitive = index > 0;
                redo.Sensitive = index < stack.Count;
                historyMenuItem.Sensitive = stack.Count > 0;

                // Stacks are always changed from the end, so first skip the subsequence that hasn't changed.
                var i = 0;
                for (; i < stack.Count && i < historyItems.Count; i++)
                {
                    var command = stack[i];
                    var (item, itemCommand) = historyItems[i];
                    item.Opacity = i < index ? 1.0 : 0.5;
                    if (command != itemCommand)
                        break;
                }
                foreach (var (item, _) in historyItems.Skip(i))
                {
                    item.Destroy();
                }
                historyItems.RemoveRange(i, historyItems.Count - i);
                for(; i < stack.Count; i++)
                {
                    var command = stack[i];
                    var item = new MenuItem(command.Description);
                    item.Opacity = i < index ? 1.0 : 0.5;
                    item.Activated += (sender, e) => window.History.GoToIndex(i);
                    historyItems.Add((item, command));
                    historyMenu.Prepend(item);
                    item.ShowAll();
                }
            };
        }
    }
}