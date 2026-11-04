using Dazed.Vitals.AssetReference;
namespace Dazed.Vitals.Paisaje;
public interface IGeometry
{
    public GemetRenderer Renderer {get;}

    public void AssignData(string pathToMaterialData, Half[] verts, Half[] texCoords, ushort[] indices);
}