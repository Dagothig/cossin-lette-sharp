using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lette.Components;
using Lette.Core;
using Lette.Resources;

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

        public delegate void CommandDelegate(ICommand command);
        public event CommandDelegate? OnApply;
        public event CommandDelegate? OnUndo;

        public delegate void OnChangeDelegate(List<ICommand> stack, int index);
        public event OnChangeDelegate? OnChange;

        public void Push(ICommand? command)
        {
            if (command == null)
                return;
            Stack.RemoveRange(Index, Stack.Count - Index);
            Stack.Add(command);
            Index++;
            command.Apply(window);
            OnApply?.Invoke(command);
            Notify();
        }

        public ICommand? Pop()
        {
            if (Index == 0)
                return null;
            Index--;
            var command = Stack[Index];
            command.Undo(window);
            OnUndo?.Invoke(command);
            Notify();
            return command;
        }

        public ICommand? Undo() => Pop();

        public ICommand? Redo()
        {
            if (Index >= Stack.Count)
                return null;
            var command = Stack[Index];
            Index++;
            command.Apply(window);
            OnApply?.Invoke(command);
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

    public class CRUDComponentCommand : ICommand
    {
        public (string, EntityDefinition) Entity;
        public IReplaceOnEntity? OldValue;
        public IReplaceOnEntity? NewValue;

        public string Description { get; set; }

        public static CRUDComponentCommand Add((string, EntityDefinition) entity, IReplaceOnEntity component) =>
            new CRUDComponentCommand(entity, $"Add ${ component.GetType().Name } to { entity.Item1 }")
            {
                NewValue = component
            };

        public static CRUDComponentCommand Remove((string, EntityDefinition) entity, IReplaceOnEntity component) =>
            new CRUDComponentCommand(entity, $"Remove ${ component.GetType().Name } from { entity.Item1 }")
            {
                OldValue = component
            };

        public static CRUDComponentCommand Update<T>((string, EntityDefinition) entity, MemberInfo field, T newValue)
        {
            var component = entity.Item2.Find(c => c.GetType() == field.DeclaringType)!;
            var newComponent = (IReplaceOnEntity)Activator.CreateInstance(field.DeclaringType!)!;
            foreach (var otherField in field.DeclaringType!.GetProperties().Cast<MemberInfo>()
                .Concat(field.DeclaringType.GetFields().Cast<MemberInfo>()))
                otherField.SetValue(newComponent, otherField.GetValue(component));
            field.SetValue(newComponent, newValue);

            return new CRUDComponentCommand(entity, $"Change { field.DeclaringType?.Name } { field.Name } to { newValue }")
            {
                OldValue = component,
                NewValue = newComponent
            };
        }

        public CRUDComponentCommand((string, EntityDefinition) entity, string description)
        {
            this.Entity = entity;
            Description = description;
        }

        void Move(EditorWindow window, IReplaceOnEntity? oldValue, IReplaceOnEntity? newValue)
        {
            if (oldValue != null)
                Entity.Item2.Remove(oldValue);
            if (newValue != null)
                Entity.Item2.Add(newValue);
            if (window.EntityRef.Value != Entity)
                window.EntityRef.Value = Entity;
            else
                window.EntityRef.Notify();
        }

        public void Apply(EditorWindow window) => Move(window, OldValue, NewValue);
        public void Undo(EditorWindow window) => Move(window, NewValue, OldValue);
    }

    public class CRUDEntityCommand : ICommand
    {
        public (string, EntityDefinition)? OldValue;
        public (string, EntityDefinition)? NewValue;

        public string Description { get; set; }

        public static CRUDEntityCommand Rename((string, EntityDefinition) entity, string newId) =>
            new CRUDEntityCommand($"Rename entity { entity.Item1 } to { newId }")
            {
                OldValue = entity,
                NewValue = (newId, entity.Item2),
            };

        public static CRUDEntityCommand Remove((string, EntityDefinition) entity) =>
            new CRUDEntityCommand($"Remove entity { entity.Item1 }")
            {
                OldValue = entity
            };

        public static CRUDEntityCommand Add((string, EntityDefinition) entity) =>
            new CRUDEntityCommand($"Add entity { entity.Item1 }")
            {
                NewValue = entity
            };

        public CRUDEntityCommand(string description)
        {
            Description = description;
        }

        void Move(EditorWindow window, (string, EntityDefinition)? prev, (string, EntityDefinition)? next)
        {
            if (prev.HasValue)
                window.LevelRef.Value!.Entities.Remove(prev.Value.Item1);
            if (next.HasValue)
                window.LevelRef.Value!.Entities[next.Value.Item1] = next.Value.Item2;
            window.EntityRef.Value = next;
        }

        public void Apply(EditorWindow window) => Move(window, OldValue, NewValue);
        public void Undo(EditorWindow window) => Move(window, NewValue, OldValue);
    }
}