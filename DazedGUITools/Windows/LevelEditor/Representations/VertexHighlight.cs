using System;
using System.Numerics;
using Dazed.Vitals.Rendering;
using OpenTK.Graphics.OpenGL;

namespace DazedGUITools.Windows.LevelEditor.Representations;

internal class VertexHighlight
{
    private static readonly int s_vao;
    private static readonly Material s_vertMaterial;

    static VertexHighlight()
    {
        s_vertMaterial = Material.GetMaterialInstance(Shader.GetNewShaderInstance("sphereShader"));
        var _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferStorage(BufferTarget.ArrayBuffer, QuadData.VertexPosAndTexturePosData.Length * sizeof(float), QuadData.VertexPosAndTexturePosData, BufferStorageFlags.None);

        var _elementBufferObject = GL.GenBuffer();
        s_vao = GL.GenVertexArray();
        GL.BindVertexArray(s_vao);
        SetAttributes();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.BufferStorage(BufferTarget.ElementArrayBuffer, QuadData.Indices.Length * sizeof(uint), QuadData.Indices, BufferStorageFlags.None);

        GL.BindVertexArray(0);
    }

    private static void SetAttributes()
    {
        int positionLocation = s_vertMaterial.GetAttribLocation("aPos");
        GL.EnableVertexAttribArray(positionLocation);
        GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

        int textCoord = s_vertMaterial.GetAttribLocation("aTexCoords");
        GL.EnableVertexAttribArray(textCoord);
        GL.VertexAttribPointer(textCoord, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
    }

    public void DrawAt(Vector3 atPosition)
    {
        GL.BindVertexArray(s_vao);
        s_vertMaterial.SetVector3("offset", atPosition);
        s_vertMaterial.SetMatrix4("model", Matrix4x4.CreateScale(MathF.Min(0.015f * Vector3.DistanceSquared(atPosition, LevelEditorCamera.Position), 0.25f)));
        s_vertMaterial.SetMatrix4("view", LevelEditorCamera.View);
        s_vertMaterial.SetMatrix4("projection", LevelEditorCamera.Projection);
        s_vertMaterial.UseMaterial();
        GL.DrawElements(PrimitiveType.Triangles, QuadData.Indices.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    } 
}