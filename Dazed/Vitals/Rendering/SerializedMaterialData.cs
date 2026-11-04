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
    public readonly string[] Uniforms;
    [JsonProperty("v")]
    public readonly object[] UniformValues;

    [JsonConstructor]
    public SerializedMaterialData(string matName, string fragmentShaderPath, string vertexShaderPath, bool isLit, string[] uniforms, object[] vals)
    {
        MaterialName = matName;
        FragmentShaderRelativePath = fragmentShaderPath;
        VertexShaderRelativePath = vertexShaderPath;
        IsLit = isLit;
        Uniforms = uniforms;
        UniformValues = vals;
    }

    [JsonIgnore]
    public string ShaderIdentification => VertexShaderRelativePath + FragmentShaderRelativePath;
}