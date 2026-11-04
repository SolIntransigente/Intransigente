using Newtonsoft.Json;

namespace Dazed.Vitals.Paisaje;

[method: JsonConstructor]
public readonly struct SerializedGeometry(Half[] vertData, Half[] texCoords, ushort[] indices, string materialDataPath)
{
    [JsonProperty("v")] public readonly Half[] VertexData = vertData;
    [JsonProperty("t")] public readonly Half[] TextureCoordinates = texCoords;
    [JsonProperty("i")] public readonly ushort[] Indices = indices;
    [JsonProperty("m")] public readonly string MaterialDataPath = materialDataPath; 
}