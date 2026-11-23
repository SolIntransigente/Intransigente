using System;
using System.Collections.Generic;
using System.Reflection;
using Dazed.Vitals.Paisaje;

namespace DazedGUITools.Windows.LevelEditor.Representations;

internal interface IDynamicRepresentation : ILESelectableObject
{
    public Type OfType { get; }
    public bool NotVisible { get; set; }
    public DynamicObjectProperty[] Properties {get;}

    public void ApplyValue(int index, object val)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Properties.Length);
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
        Properties[index].SetValue(val);    
    }
    public void Destroy();
    public void Draw();
}