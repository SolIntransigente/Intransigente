using Newtonsoft.Json;

namespace Dazed.Vitals.Paisaje;

public readonly struct SerializedPaisaje(SerializedPaisajeElement[] elements, string[] context)
{
    // ? May be better to change this struct so it contains dynamic objects and static geometry separated
    [JsonProperty("b")] public readonly SerializedPaisajeElement[] Elements = elements;
    [JsonProperty("c")] public readonly string[] Context = context;
}