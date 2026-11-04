using Dazed.Vitals.AssetReference;
using Dazed.Vitals.Rendering;

namespace Dazed.Vitals.Paisaje;

/// <summary>
/// (Ge)o(met)ry object present in a paisaje instance, contains all the vertices that use certain material inside a section.
/// </summary>
public class Gemet : PaisajeObject, IGeometry
{
    public GemetRenderer Renderer => m_renderer;
    private GemetRenderer m_renderer = null!;

    public void AssignData(string pathToMaterialData, Half[] verts, Half[] texCoords, ushort[] indices)
    {
        m_renderer = new(pathToMaterialData, verts, texCoords, indices);
    }
}