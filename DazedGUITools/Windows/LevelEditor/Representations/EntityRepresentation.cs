using System;
using System.Collections.Generic;
using System.Reflection;
using Dazed.Vitals;
using Dazed.Vitals.Paisaje;

namespace DazedGUITools.Windows.LevelEditor.Representations;

internal class EntityRepresentation : IDynamicRepresentation
{
    public Type OfType { get; private set; }
    public DynamicObjectProperty[] Properties { get; private set; }

    public bool NotVisible { get; set; }

    public ObjectType ObjType => ObjectType.DynamicObject;

    public EntityRepresentation(Type ofType)
    {
        if (!ofType.IsSubclassOf(typeof(Entity)))
            throw new ArgumentException("Type specified is not of an entity", nameof(ofType));
        OfType = ofType;
        Properties = GetAllTypeProperties();
    }

    public DynamicObjectProperty[] GetAllTypeProperties()
    {
        List<DynamicObjectProperty> props = new();
        var fields = OfType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        foreach(var field in fields)
        {
            var att = field.GetCustomAttribute<PaisajeExposeAttribute>();
            if (att == null)
                continue;
            if (!GetAppropriateProperty(field, out var obj))
                continue;
            props.Add(obj);
        }
        return [.. props];
    }

    private static bool GetAppropriateProperty(FieldInfo field, out DynamicObjectProperty obj)
    {
        try
        {
            obj = new(field.Name, CommonUtilities.GetDefaultOfStruct(field.FieldType), field.FieldType);
            return true;
        }
        catch (Exception ex)
        {
            obj = null!;
            Scriptorium.LogException(ex);
            return false;
        }
    }

    public void Destroy()
    {
    }

    public void Draw()
    {
    }
}