using System.Numerics;
using Dazed.Vitals.Rendering;
using OpenTK.Graphics.OpenGL;

namespace Dazed.Vitals.Paisaje;

public class GemetRenderer : Renderable
{
    private const byte ByteSizeOfHalf = 2;
    public override bool AllowRendering => true;
    private readonly ushort m_indicesCount;

    public Material GemetMaterial => m_mat;
    private readonly Material m_mat;

    private readonly int m_vao;

    public GemetRenderer(string pathToMaterialData, Half[] verts, Half[] texCoords, ushort[] indices)
    {
        if(verts == null || verts.Length == 0 || texCoords == null || texCoords.Length == 0 || indices == null || indices.Length == 0)
        {
            Scriptorium.Error("A paisaje geometry object tried to be created with empty or null arguments");
            Scriptorium.Error($"verts array is <{(verts == null ? "null" : "empty")}>");
            Scriptorium.Error($"Texture coordinates array is <{(texCoords == null ? "null" : "empty")}>");
            Scriptorium.Error($"Indices array is <{(indices == null ? "null" : "empty")}>");
            throw new NullReferenceException();
        }
        
        var arrayBufferData = ArrangeData(verts, texCoords);

        var vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferStorage(BufferTarget.ArrayBuffer, arrayBufferData.Length * ByteSizeOfHalf, arrayBufferData, BufferStorageFlags.None);

        var elementBufferObject = GL.GenBuffer();
        m_vao = GL.GenVertexArray();
        GL.BindVertexArray(m_vao);
        
        var data = Cereal.SafeLoadData<SerializedMaterialData>(pathToMaterialData, out bool successful); 

        m_mat = successful ? Material.GetMaterialInstance(data) : Material.MissingMaterial;
        m_indicesCount = (ushort) indices.Length;
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
        GL.BufferStorage(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(ushort), indices, BufferStorageFlags.None);

        GL.BindVertexArray(0);
    }

    private static Half[] ArrangeData(Half[] verts, Half[] texCoords)
    {
        Half[] vertData = new Half[verts.Length + texCoords.Length];
        short positionOffset = 0, texCoordOffset = 0;
        for(short i = 0; i < vertData.Length;)
        {
            short offset = 0;
            //Position
            vertData[i + offset++] = verts[i + positionOffset++];
            vertData[i + offset++] = verts[i + positionOffset++];
            vertData[i + offset++] = verts[i + positionOffset++];
            //Texture coordinates
            vertData[i + offset++] = texCoords[i + texCoordOffset++];
            vertData[i + offset++] = texCoords[i + texCoordOffset++];
            i += offset;
        }
        return vertData;
    }

    public override void Render(in RenderingData data)
    {
        if (m_mat == null)
        {
            if (DebugState.DebugLow)
                Scriptorium.Warn("A mesh can not be drawn because it has no shader");
            return;
        }
        Artist.SetCommonValues(m_mat, in data);
        GL.BindVertexArray(m_vao);
        m_mat.UseMaterial();
        GL.DrawElements(PrimitiveType.Triangles, m_indicesCount, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }

    public new void Dispose()
    {
        base.Dispose();
        GL.DeleteVertexArray(m_vao);
    }
}