using Newtonsoft.Json;

namespace Dazed.Vitals.Rendering;

public readonly struct SerializedMaterialData
{
    [JsonProperty("n")]
    public readonly string MaterialName;
    [JsonProperty("fs")]
    public readonly string FragmentShaderRelativePath;
    [JsonProperty("vs")]
    public readonly string VertexShaderRelativePath;
    [JsonProperty("l")]
    public readonly bool IsLit;
    [JsonProperty("u")]
    public readonly SerializedMaterialProperty[] Properties;

    [JsonConstructor]
    public SerializedMaterialData(string matName, string fragmentShaderPath, string vertexShaderPath, bool isLit, SerializedMaterialProperty[] properties)
    {
        MaterialName = matName;
        FragmentShaderRelativePath = fragmentShaderPath;
        VertexShaderRelativePath = vertexShaderPath;
        IsLit = isLit;
        Properties = properties;
    }

    [JsonIgnore]
    public string ShaderIdentification => VertexShaderRelativePath + FragmentShaderRelativePath;
}