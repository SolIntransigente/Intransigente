using Newtonsoft.Json;

namespace Dazed.Vitals.Paisaje;

[method: JsonConstructor]
public readonly struct SerializedDynamic(string typeName, string[] fieldsAndParameters, object[] values, int indexToGeometry = -1)
{
    [JsonProperty("i")] public readonly int IndexToGeometry = indexToGeometry; 
    [JsonProperty("t")] public readonly string TypeName = typeName;
    [JsonProperty("f")] public readonly string[] FieldsAndParameters = fieldsAndParameters;
    [JsonProperty("v")] public readonly object[] Values = values;

    [JsonIgnore] public bool AssignedIndex => IndexToGeometry > -1;
}