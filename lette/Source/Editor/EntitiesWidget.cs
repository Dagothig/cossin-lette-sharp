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

        public EntitiesWidget(EditorWindow window) : base(Orientation.Horizontal, 0)
        {
            this.window = window;

            TreeView entitiesChooser = new();
            entitiesChooser.WidthRequest = 200;

            entitiesChooser.AppendColumn("Name", new CellRendererText(), "text", 0);

            ListStore entitiesList = new(typeof(string), typeof(string));
            Dictionary<string, TreeRowReference> entityRows = new();

            window.LevelRef.OnChange += level =>
            {
                entitiesList.Clear();
                entityRows.Clear();
                if (level == null)
                    return;
                foreach (var nameEntity in level.Entities)
                {
                    var iter = entitiesList.AppendValues(nameEntity.Key);
                    entityRows[nameEntity.Key] = new TreeRowReference(
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
                    window.EntityRef.Value = (name, window.LevelRef.Value!.Entities[name]);
                }
            };
            window.EntityRef.OnChange += nameEntity =>
            {
                if (nameEntity.HasValue)
                    entitiesChooser.Selection.SelectPath(entityRows[nameEntity.Value.Item1].Path);
                else
                    entitiesChooser.Selection.UnselectAll();
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

            window.EntityRef.OnChange += nameEntity =>
            {
                foreach (var (type, widget) in componentTypeWidgets)
                {
                    IReplaceOnEntity? component = nameEntity?.Item2.Find(c => c.GetType() == type);
                    if (component != null)
                    {
                        if (!widget.IsVisible)
                            widget.Show();
                        widget.Update(nameEntity!.Value, component);
                    }
                    else
                        widget.Hide();
                }
            };
        }
    }

    public class AutoComponentWidget: Box
    {
        EditorWindow window;
        FieldWidget[] fieldWidgets;

        public AutoComponentWidget(EditorWindow window, Type type) : base(Orientation.Vertical, 0)
        {
            this.window = window;
            var label = new Label(type.Name);
            label.Xalign = 0;
            PackStart(label, false, false, 0);

            fieldWidgets = type
                .GetProperties().Cast<MemberInfo>()
                .Concat(type.GetFields().Cast<MemberInfo>())
                .Where(field => !field.IsDefined(typeof(JsonIgnoreAttribute), true))
                .Select<MemberInfo, FieldWidget>(field => 
                {
                    var type = field.MemberType();
                    if (type == typeof(int))
                        return new IntFieldWidget(window, field);
                    else if (type == typeof(float))
                        return new FloatFieldWidget(window, field);
                    else if (type == typeof(Vector2))
                        return new Vector2FieldWidget(window, field);
                    else if (type == typeof(string))
                        return new StringFieldWidget(window, field);
                    else 
                        return new UnsupportedFieldWidget(window, field);
                })
                .ToArray();
            foreach (var fieldWidget in fieldWidgets)
                PackStart(fieldWidget, false, true, 0);
        }

        public void Update((string, EntityDefinition) entity, IReplaceOnEntity component)
        {
            foreach (var fieldWidget in fieldWidgets)
                fieldWidget.Update(entity, component);
        }
    }

    public abstract class FieldWidget : Box
    {
        internal EditorWindow window;
        internal MemberInfo field;

        public abstract void Update((string, EntityDefinition) entity, IReplaceOnEntity component);

        public FieldWidget(EditorWindow window, MemberInfo field) : base(Orientation.Horizontal, 8)
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
        }
    }

    public abstract class FieldWidget<T> : FieldWidget where T: IEquatable<T>
    {
        internal (string, EntityDefinition)? entity;
        internal T receivedValue;

        public abstract void UpdateValue();
        internal abstract T defaultValue { get; }

        protected FieldWidget(EditorWindow window, MemberInfo field) : base(window, field) 
        {
            receivedValue = defaultValue;
        }

        public override void Update((string, EntityDefinition) entity, IReplaceOnEntity component)
        {
            this.entity = entity;
            var newValue = (T)(field.GetValue(component) ?? defaultValue);
            if (!newValue.Equals(receivedValue))
            {
                receivedValue = newValue;
                UpdateValue();
            }
        }

        public void ValueChanged(T newValue)
        {
            if (!newValue.Equals(receivedValue) && entity.HasValue)
            {
                receivedValue = newValue;
                window.History.Push(CRUDComponentCommand.Update<T>(entity.Value, field, receivedValue));
            }
        }
        
    }

    public class UnsupportedFieldWidget : FieldWidget
    {
        public UnsupportedFieldWidget(EditorWindow window, MemberInfo field) : base(window, field)
        {
            var err = new Label($"Unsupported type { field.MemberType().ToString() }");
            err.LineWrap = true;
            err.Wrap = true;
            err.Xalign = 0;
            err.LineWrapMode = Pango.WrapMode.WordChar;
            PackStart(err, false, true, 0);
        }

        public override void Update((string, EntityDefinition) entity, IReplaceOnEntity component) {}
    }

    public class IntFieldWidget : FieldWidget<int>
    {
        SpinButton entry;

        public IntFieldWidget(EditorWindow window, MemberInfo field) : base(window, field)
        {
            entry = new(int.MinValue, int.MaxValue, 1);
            entry.Value = 0;
            entry.IsFloating = false;
            entry.ValueChanged += (_, _) => ValueChanged(entry.ValueAsInt);
            PackStart(entry, true, true, 0);
        }

        internal override int defaultValue => 0;
        public override void UpdateValue() => entry.Value = receivedValue;
    }

    public class FloatFieldWidget : FieldWidget<float>
    {
        SpinButton entry;

        public FloatFieldWidget(EditorWindow window, MemberInfo field) : base(window, field)
        {
            entry = new(int.MinValue, int.MaxValue, 1);
            entry.Value = 0;
            entry.IsFloating = true;
            entry.ValueChanged += (_, _) => ValueChanged((float)entry.Value);
            PackStart(entry, true, true, 0);
        }

        internal override float defaultValue => 0;

        public override void UpdateValue() => entry.Value = receivedValue;
    }

    public class StringFieldWidget : FieldWidget<string>
    {
        Entry entry;

        public StringFieldWidget(EditorWindow window, MemberInfo field) : base(window, field)
        {
            entry = new();
            entry.Changed += (_, _) => ValueChanged(entry.Text);
            PackStart(entry, true, true, 0);
        }

        internal override string defaultValue => "";
        public override void UpdateValue() => entry.Text = receivedValue;
    }

    public class Vector2FieldWidget : FieldWidget<Vector2>
    {
        SpinButton x, y;

        public Vector2FieldWidget(EditorWindow window, MemberInfo field) : base(window, field)
        {
            x = new(int.MinValue, int.MaxValue, 1); 
            y = new(int.MinValue, int.MaxValue, 1);
            x.Value = y.Value = 0;
            x.IsFloating = y.IsFloating = true;
            x.WidthChars = y.WidthChars = 4;
            x.Digits = y.Digits = 4;
            EventHandler changedHandler = (_, _) => ValueChanged(new Vector2((float)x.Value, (float)y.Value));
            x.ValueChanged += changedHandler;
            y.ValueChanged += changedHandler;
            PackStart(x, true, true, 0);
            PackStart(y, true, true, 0);
        }

        internal override Vector2 defaultValue => Vector2.Zero;
        public override void UpdateValue()
        {
            var originalReceived = receivedValue;
            receivedValue = new Vector2(receivedValue.X, (float)y.Value);
            x.Value = receivedValue.X;
            receivedValue = originalReceived;
            y.Value = receivedValue.Y;
        }
    }
}