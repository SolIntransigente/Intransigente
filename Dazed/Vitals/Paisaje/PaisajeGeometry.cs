using System.Numerics;
using Dazed.Vitals.Rendering;

namespace Dazed.Vitals.Paisaje;
/// <summary>
/// The building block for the geometry of the paisaje
/// </summary>
public sealed class PaisajeGeometry : Renderable
{
    private readonly string _name;
    public readonly Half[] Vertices;
    public readonly ushort[] Indices;

    public Material? GeometryMaterial { get; set; }

    public readonly PaisajeDynamicBrush? BrushBehavior; 
    
    public PaisajeGeometry(SerializedPaisajeElement sb)
    {
        Mundo.RetrieveGeometryData(sb, out Vertices, out Indices);
        _name = sb.ElementType;
    }

    public PaisajeGeometry(SerializedPaisajeElement sb, PaisajeDynamicBrush pair)
    {
        BrushBehavior = pair;
        Mundo.RetrieveGeometryData(sb, out Vertices, out Indices);
        _name = sb.ElementType;
    }
    
    public override string ToString() => $"{base.ToString()} of name: {_name}";
    public override bool AllowRendering => true;
    public override void Render(in RenderingData data)
    {
        var world = BrushBehavior == null ? Matrix4x4.Identity : BrushBehavior.Transform.World;
        GeometryMaterial.UseMaterial();
    }
}