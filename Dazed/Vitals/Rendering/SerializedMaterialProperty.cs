using Newtonsoft.Json;

namespace Dazed.Vitals.Rendering;

[method: JsonConstructor]
public readonly struct SerializedMaterialProperty(string uniformName, string uniformType)
{
    [JsonProperty("n")]
    public readonly string UniformName = uniformName;
    [JsonProperty("t")]
    public readonly string UniformType = uniformType;
}