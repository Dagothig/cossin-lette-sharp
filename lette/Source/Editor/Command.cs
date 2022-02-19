using System.Collections.Generic;
using System.ComponentModel;
using Gtk;
using Lette.Components;
using Lette.Resources;
using System.Linq;
using System;
using System.Reflection;
using Lette.Core;

namespace Lette.Editor
{
    public class HistoryStack
    {
        EditorWindow window;
        public int Index = 0;
        public List<ICommand> Stack = new();

        public HistoryStack(EditorWindow window)
        {
            this.window = window;
        }

        public delegate void onChangeDelegate(List<ICommand> stack, int index);
        public event onChangeDelegate? OnChange;

        public void Push(ICommand? command)
        {
            if (command == null)
                return;
            Stack.RemoveRange(Index, Stack.Count -  Index);
            Stack.Add(command);
            Index++;
            command.Apply(window);
            Notify();
        }

        public ICommand? Pop()
        {
            if (Index == 0)
                return null;
            Index--;
            var command = Stack[Index];
            command.Undo(window);
            Notify();
            return command;
        }

        public ICommand? Undo() => Pop();

        public ICommand? Redo()
        {
            if (Index >= Stack.Count) 
                return null;
            var command = Stack[Index];
            command.Apply(window);
            Index++;
            Notify();
            return command;
        }

        public void GoToIndex(int index)
        {
            while (Index < index)
                Redo();
            while (Index > index)
                Undo();
        }

        public void Clear() 
        {
            Index = 0;
            Stack.Clear();
            Notify();
        }

        public void Notify() 
        {
            OnChange?.Invoke(Stack, Index);
        }
    }

    public interface ICommand
    {
        string Description { get; }
        void Apply(EditorWindow window);
        void Undo(EditorWindow window);
    }

    public class UpdateComponentCommand<T> : ICommand
    {
        IReplaceOnEntity component;
        MemberInfo field;
        T newValue;
        T oldValue;
        public string Description { get; set; }

        public static UpdateComponentCommand<T>? For(EntityDefinition entity, MemberInfo field, T newValue)
        {
            var component = entity.Find(c => c.GetType() == field.DeclaringType)!;
            var oldValue = (T)field.GetValue(component)!;
            return (newValue?.Equals(oldValue) ?? oldValue == null) ? 
                null :
                new UpdateComponentCommand<T>(component, field, newValue, oldValue);
        }

        UpdateComponentCommand(IReplaceOnEntity component, MemberInfo field, T newValue, T oldValue)
        {
            this.component = component;
            this.field = field;
            this.newValue = newValue;
            this.oldValue = oldValue;
            Description = $"Change { field.DeclaringType?.Name } { field.Name }";
        }

        public void Apply(EditorWindow window)
        {
            /*field.SetValue(component, newValue);
            window.EntityRef.Notify();*/
        }

        public void Undo(EditorWindow window)
        {
            /*field.SetValue(component, oldValue);
            window.EntityRef.Notify();*/
        }
    }
}