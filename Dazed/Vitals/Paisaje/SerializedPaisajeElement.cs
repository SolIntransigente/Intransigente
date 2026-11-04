using Newtonsoft.Json;

namespace Dazed.Vitals.Paisaje;

[method: JsonConstructor]
public readonly struct SerializedPaisajeElement(
    string elementName,
    string elementType,
    SerializedElementData elementData,
    PaisajeElementType paisajeElementType)
{
    [JsonProperty("n")]
    public readonly string ElementName = elementName;
    [JsonProperty("t")]
    public readonly string ElementType = elementType;
    [JsonProperty("d")]
    public readonly SerializedElementData ElementData = elementData;
    [JsonProperty("c")]
    public readonly PaisajeElementType PaisajeElementType = paisajeElementType;
}