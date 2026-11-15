using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dazed.Vitals.Rendering;

[JsonConverter(typeof(StringEnumConverter))]
public enum TextureType
{
    Texture1D,
    Texture2D,
    Texture3D,
    CubeMap,
    Texture1DArray,
    Texture2DArray,
    Texture3DArray,
}