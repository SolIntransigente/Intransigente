using Dazed.Vitals.AssetReference;
using Dazed.Vitals.Rendering;

namespace Dazed.Vitals.Paisaje;

public interface IEntity
{
    public AssetPath PathToMesh {get;}
    public AssetPath PathToMaterialData {get;}
    public Mesh? InstantiatedMesh {get;}
    public string Identifier {get;}
}