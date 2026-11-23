using System;
using Dazed.Vitals;

namespace DazedGUITools.Windows.LevelEditor.Representations;

internal class DynamicObjectProperty
{
    public readonly string ParameterName;
    public readonly Type ValueType;
    internal object m_paramValue;
    protected object ParameterValue
    {
        get => m_paramValue;
        set => m_paramValue = value;
    }
    public DynamicObjectProperty(string name, object defaultValue, Type valueType)
    {
        // ? Could add an interface that marks a class as supported
        if (!valueType.IsValueType)
            Scriptorium.Warn($"Property of name '{ParameterName}' may not be displayed because it is a class type and not a value type.");
        ParameterName = name;
        m_paramValue = defaultValue;
        ValueType = valueType;
    }
    public void SetValue(object value)
    {
        Type typeOfAssigned = value.GetType();
        if (typeOfAssigned != ValueType)
        {
            Scriptorium.Error($"There is a value mismatch between the value type of parameter [{ParameterName} | Of type {ValueType}] and the new value of type {typeOfAssigned}");
            return;
        }
        ParameterValue = value;
    }
}

// internal class GenericDynamicObjectProperty<T> : DynamicObjectProperty where T : struct
// {
//     private T m_propertyValue;
//     public T GenericParameterValue => m_propertyValue;
//     protected override object ParameterValue { get => m_propertyValue; set => m_propertyValue = (T)value; }

//     public GenericDynamicObjectProperty(string name) : base(name, typeof(T))
//     {
//         m_propertyValue = default;
//     }
//     public override void SetValue(object value)
//     {
//         if (value.GetType() != ValueType)
//             Scriptorium.Error($"There is a value mismatch between the value type of parameter [{ParameterName} | Of type {ValueType.Name}] and the new value of type {value.GetType().Name}");
//         SetValue((T)value);
//     }
    
//     public void SetValue(T value)
//     {
//         m_propertyValue = value;
//     }
// }