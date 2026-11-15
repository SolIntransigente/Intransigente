using System.Data.Common;
using Dazed.Vitals.AssetReference;
using Newtonsoft.Json;

namespace Dazed.Vitals.Rendering;

public readonly struct TexturePath
{
    [JsonProperty("i")]
    public readonly TextureIdentification Identification;
    public static AssetPath ToAssetPath(TexturePath path) => new(path.Identification.Path);

    public TexturePath()
    {
        Identification = TextureIdentification.Default2D;
    }

    [JsonConstructor]
    public TexturePath(TextureIdentification textureIdentification)
    {
        Identification = textureIdentification;
    }
}