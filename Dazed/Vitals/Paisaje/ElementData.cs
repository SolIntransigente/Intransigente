using Newtonsoft.Json;

namespace Dazed.Vitals.Paisaje;

[method: JsonConstructor]
public readonly struct SerializedElementData(SerializedDynamic? dynamic, SerializedGeometry? geometry)
{
	[JsonProperty("g")] public readonly SerializedDynamic? Dynamic = dynamic;
	[JsonProperty("d")] public readonly SerializedGeometry? Geometry = geometry;
}