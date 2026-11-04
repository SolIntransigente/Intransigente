using System.Numerics;

namespace Dazed.Vitals.Paisaje;

/// <summary>
/// (Di)namic (Ge)o(me)try that is present inside a paisaje, is constructed in the level editor but functions like an entity
/// </summary>
public abstract class Digemet : PaisajeObject, IDynamic, IGeometry
{
    public Transform Transformation => m_transform;
    private Transform m_transform = null!;

    public GemetRenderer Renderer => m_renderer;

    public bool Exists => m_exists;
    private bool m_exists;

    private GemetRenderer m_renderer = null!;

    public void AssignData(string pathToMaterialData, Half[] verts, Half[] texCoords, ushort[] indices)
    {
        Vector3 center = Vector3.Zero;
        for(short i = 0; i < verts.Length;)
        {
            short offset = 0;
            center += new Vector3((float)verts[i + offset++], (float)verts[i + offset++], (float)verts[i + offset++]);
            i += offset;
        }
        center /= verts.Length / 3f;
        Half[] newVerts = new Half[verts.Length];
        //Offsets vertex positions to then assign the position of the Gemet at the center of all its vertex
        for(int v = 0; v < verts.Length;)
        {
            short offset = 0;
            newVerts[v + offset] = (verts[v + offset++] - (Half)center.X);
            newVerts[v + offset] = (verts[v + offset++] - (Half)center.Y);
            newVerts[v + offset] = (verts[v + offset++] - (Half)center.Z);
            v += offset;
        }

        m_renderer = new(pathToMaterialData, newVerts, texCoords, indices);
        m_transform = new () {Position = center};
        m_exists = true;
    }

    public void Destroy()
    {
        if (!m_exists)
            return;
        m_exists = false;
        m_renderer.Dispose();
    }
}