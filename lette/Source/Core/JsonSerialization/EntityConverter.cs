using System.Collections.Generic;
using System;
using System.Text.Json;
using Lette.Resources;
using System.Reflection;
using System.Linq;
using Lette.Components;

namespace Lette.Core.JsonSerialization
{
    public class EntityConverter : MapConverter<EntityDefinition>
    {
        static readonly Dictionary<string, Type> ComponentTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IReplaceOnEntity)))
            .ToDictionary(value => value.Name.ToCamel());

        public override EntityDefinition Init() => new EntityDefinition();

        public override void ReadValue(
            ref Utf8JsonReader reader,
            ref EntityDefinition def,
            string name,
            JsonSerializerOptions options)
        {
            var type = ComponentTypes[name];
            var value = JsonSerializer.Deserialize(ref reader, type, options) as IReplaceOnEntity;
            if (value == null)
                throw new Exception();
            def.Add(value);
        }

        public override void Write(Utf8JsonWriter writer, EntityDefinition value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
