using Dazed.Vitals.Rendering;
using Newtonsoft.Json;

namespace Dazed.Vitals.Paisaje
{
    public struct PaisajeVariables(float ambientLightIntensity, Color ambientLightColor, string? skyboxPath, string[] context)
    {
        [JsonProperty("li")] public float AmbientLightIntensity = ambientLightIntensity;
        [JsonProperty("lc")] public Color AmbientLightColor = ambientLightColor;
        [JsonProperty("s")] public string? SkyboxPath = skyboxPath;
        [JsonProperty("c")] public string[] Context = context;
    }
}
