using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;
using Gtk;
using Lette.Components;
using Lette.Resources;
using System.Reflection;
using Lette.Core;
using Microsoft.Xna.Framework;

namespace Lette.Editor
{
    public class EntitiesWidget : Box
    {
        EditorWindow window;
        Dictionary<EntityDefinition, TreeRowReference> EntityRows = new();

        public EntitiesWidget(EditorWindow window) : base(Orientation.Horizontal, 0)
        {
            this.window = window;

            TreeView entitiesChooser = new();
            entitiesChooser.WidthRequest = 200;

            entitiesChooser.AppendColumn("Name", new CellRendererText(), "text", 0);

            ListStore entitiesList = new(typeof(string), typeof(string));
            window.LevelRef.OnChange += level =>
            {
                entitiesList.Clear();
                if (level == null)
                    return;
                foreach (var nameEntity in level.Entities)
                {
                    var iter = entitiesList.AppendValues(nameEntity.Key);
                    EntityRows[nameEntity.Value] = new TreeRowReference(
                        entitiesList,
                        entitiesList.GetPath(iter));
                }
            };

            entitiesChooser.Model = entitiesList;

            entitiesChooser.CursorChanged += (sender, e) =>
            {
                foreach (var path in entitiesChooser.Selection.GetSelectedRows())
                {
                    entitiesList.GetIter(out TreeIter iter, path);
                    string name = (string)entitiesList.GetValue(iter, 0);
                    window.EntityRef.Value = window.LevelRef.Value!.Entities[name];
                }
            };
            window.EntityRef.OnChange += entity =>
            {
                if (entity == null)
                    entitiesChooser.Selection.UnselectAll();
                else
                    entitiesChooser.Selection.SelectPath(EntityRows[entity].Path);
            };

            PackStart(entitiesChooser, false, false, 0);

            var separator = new Separator(Orientation.Vertical);
            PackStart(separator, false, false, 0);

            ScrolledWindow componentsScroll = new();
            componentsScroll.WidthRequest = 400;
            ComponentsWidget componentsWidget = new(window);
            componentsWidget.Margin = 16;
            componentsScroll.Add(componentsWidget);

            PackStart(componentsScroll, false, false, 0);
        }
    }

    public class ComponentsWidget : Box
    {
        EditorWindow window;
        Dictionary<Type, AutoComponentWidget> componentTypeWidgets;

        public ComponentsWidget(EditorWindow window) : base(Orientation.Vertical, 16)
        {
            this.window = window;

            componentTypeWidgets = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsValueType && type.IsAssignableTo(typeof(IReplaceOnEntity)))
                .ToDictionary(type => type, type =>  new AutoComponentWidget(window, type));
            
            foreach (var (type, widget) in componentTypeWidgets)
            {
                PackStart(widget, false, true, 0);
            }

            window.EntityRef.OnChange += entity =>
            {
                foreach (var (type, widget) in componentTypeWidgets)
                    widget.Hide();
                if (entity == null)
                    return;
                foreach (var component in entity)
                {
                    var type = component.GetType();
                    if (componentTypeWidgets.ContainsKey(type))
                    {
                        var widget = componentTypeWidgets[type];
                        widget.Update(entity, component);
                        widget.Show();
                    }
                    else
                        window.Alert($"Component type not supported { type.Name }");
                }
            };
        }
    }

    public class AutoComponentWidget: Box
    {
        EditorWindow window;
        AutoComponentFieldWidget[] fieldWidgets;

        public AutoComponentWidget(EditorWindow window, Type type) : base(Orientation.Vertical, 0)
        {
            this.window = window;
            var label = new Label(type.Name);
            label.Xalign = 0;
            PackStart(label, false, false, 0);

            fieldWidgets = type
                .GetProperties().Cast<MemberInfo>()
                .Concat(type.GetFields().Cast<MemberInfo>())
                .Where(c =>
                    !c.GetCustomAttributes(true).Any(a => 
                        a.GetType().IsAssignableTo(typeof(JsonIgnoreAttribute))))
                .ToArray()
                .Select(field => new AutoComponentFieldWidget(window, field)).ToArray();
            foreach (var fieldWidget in fieldWidgets)
                PackStart(fieldWidget, false, true, 0);
        }

        public void Update(EntityDefinition entity, IReplaceOnEntity component)
        {
            foreach (var fieldWidget in fieldWidgets)
                fieldWidget.Update(entity, component);
        }
    }

    public class AutoComponentFieldWidget : Box
    {
        EditorWindow window;
        MemberInfo field;
        EntityDefinition? entity;
        public Action<IReplaceOnEntity> UpdateComponent;
        public void Update(EntityDefinition entity, IReplaceOnEntity component)
        {
            this.entity = entity;
            UpdateComponent(component);
        }

        public AutoComponentFieldWidget(EditorWindow window, MemberInfo field) : base(Orientation.Horizontal, 8)
        {
            this.window = window;
            this.field = field;

            var label = new Label(field.Name);
            label.Justify = Justification.Right;
            label.WidthChars = 6;
            label.MaxWidthChars = 6;
            label.Ellipsize = Pango.EllipsizeMode.End;
            label.Xalign = 1;
            PackStart(label, false, true, 0);

            var type = field.MemberType();
            if (type == typeof(int) || type == typeof(float)) 
            {
                SpinButton entry = new(int.MinValue, int.MaxValue, 1), y = new(int.MinValue, int.MaxValue, 1);
                entry.IsFloating = type == typeof(float);
                entry.ValueChanged += (sender, e) =>
                {
                    if (entity != null)
                        window.History.Push(
                            type == typeof(float) ?
                                UpdateComponentCommand<float>.For(entity, field, (float)entry.Value) :
                                UpdateComponentCommand<int>.For(entity, field, entry.ValueAsInt));
                };
                PackStart(entry, true, true, 0);
                UpdateComponent = component => entry.Value = field.GetValue(component) as double? ?? 0.0;
            }
            else if (type == typeof(string))
            {
                var entry = new Entry();
                PackStart(entry, true, true, 0);
                UpdateComponent = component => entry.Text = field.GetValue(component)?.ToString() ?? "";
            }
            else if (type == typeof(Vector2))
            {
                SpinButton x = new(int.MinValue, int.MaxValue, 1), y = new(int.MinValue, int.MaxValue, 1);
                x.IsFloating = y.IsFloating = true;
                x.WidthChars = y.WidthChars = 4;
                x.Digits = y.Digits = 4;
                PackStart(x, true, true, 0);
                PackStart(y, true, true, 0);
                UpdateComponent = component =>
                {
                    var vec2 = field.GetValue(component) as Vector2? ?? Vector2.Zero;
                    x.Value = vec2.X;
                    y.Value = vec2.Y;
                };
            } 
            else
            {
                var err = new Label($"Unsupported type { type.ToString() }");
                err.LineWrap = true;
                err.Wrap = true;
                err.Xalign = 0;
                err.LineWrapMode = Pango.WrapMode.WordChar;
                PackStart(err, false, true, 0);
                UpdateComponent = _ => {};
            }
        }
    }
}