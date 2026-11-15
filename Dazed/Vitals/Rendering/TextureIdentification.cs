using System;
using System.Collections.Generic;
using System.Text;
using Dazed.Vitals.AssetReference;
using Newtonsoft.Json;

namespace Dazed.Vitals.Rendering
{
    public readonly struct TextureIdentification
    {
        [JsonProperty("p")]
        public readonly string Path;
        [JsonProperty("dt")]
        public readonly ImageDataType ImageDataType;
        [JsonProperty("tt")]
        public readonly TextureType TextureType;
        [JsonProperty("a")]
        public readonly bool AlphaIsTransparency;
        [JsonProperty("r")]
        public readonly bool IsReadOnly;

        [JsonConstructor]
        public TextureIdentification(string path, ImageDataType imageDataType, TextureType tType, bool alphaIsTransparency = true, bool isReadOnly = true)
        {
            Path = path;
            ImageDataType = imageDataType;
            TextureType = tType;
            AlphaIsTransparency = alphaIsTransparency;
            IsReadOnly = isReadOnly;
        }

        public static TextureIdentification Default2D { get; } = new TextureIdentification(AssetPath.NoPathAssigned, ImageDataType.RGB, TextureType.Texture2D);
    }
}
