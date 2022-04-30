using System;

namespace ShaderlabVS.Data;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
internal sealed class DefinitionKeyAttribute : Attribute
{
    public string Name { get; set; }

    public DefinitionKeyAttribute(string name) => Name = name;
}
