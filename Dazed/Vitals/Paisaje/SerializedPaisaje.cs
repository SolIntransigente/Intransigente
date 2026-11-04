using Newtonsoft.Json;

namespace Dazed.Vitals.Paisaje;

[method: JsonConstructor]
public readonly struct SerializedPaisaje(SerializedElementData[] elements, PaisajeVariables variables)
{
    [JsonProperty("e")] public readonly SerializedElementData[] Elements = elements;
    [JsonProperty("v")] public readonly PaisajeVariables PaisajeVariables = variables;
}