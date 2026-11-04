using Dazed.Vitals.AssetReference;
using Dazed.Vitals.Rendering;

namespace Dazed.Vitals.Paisaje;

public abstract class Entity : PaisajeObject, IDynamic, IEntity
{
    public Transform Transformation => m_transform;
    protected Transform m_transform = null!;
    public abstract AssetPath PathToMesh {get;}
    public abstract AssetPath PathToMaterialData {get;}
    public string Identifier => $"{ObjectSharedName}: {ObjectUniqueIdentifier}";
    //TODO: implement this
    public abstract string ObjectSharedName {get; protected set;}

    /// <summary>
    /// Equivalent to <seealso cref="Exists"/>, marks that it's not disposed and if it has been initialized
    /// </summary>
    public bool Available => m_initialized && !m_disposed;
    public bool m_disposed;
    public bool m_initialized;

    public Mesh? InstantiatedMesh => m_mesh;

    /// <summary>
    /// Equivalent to <seealso cref="Available"/>, marks that it's not disposed and if it has been initialized
    /// </summary>
    public bool Exists => Available;

    protected Mesh? m_mesh;
    protected Material? m_material;
    protected override void OnCreation()
    {
        m_transform = new();
        var data = Cereal.SafeLoadData<SerializedMaterialData>(PathToMaterialData, out bool successful);
        if(!successful)
        {
            Scriptorium.Error($"Entity <{Identifier}> failed to load its material data, check console for more information.");
            m_material = Material.MissingMaterial;
        }
        else
            m_material = Material.GetMaterialInstance(data);
        m_mesh = Mesh.GetMeshInstance(PathToMesh.AssetRelativePath, m_material);
        m_initialized = true;
    }

    public void Destroy()
    {
        if(m_disposed)
            return;
        m_disposed = true;
        m_mesh?.Dispose();
        Phone.UnSubscribeToApplicationQuit(Destroy);
        DestroyCall();
    }
    protected abstract void DestroyCall();
}