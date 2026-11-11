using System;
using System.Numerics;
using Dazed.Vitals;
using Dazed.Vitals.Rendering;
using OpenTK.Graphics.OpenGL;

namespace DazedGUITools.Windows.LevelEditor.Representations;

internal class VertexHighlight
{
    private static readonly int s_vao;
    private static readonly Material s_vertMaterial;

    static VertexHighlight()
    {
        var previousState = PathUtils.IsBasePathOverwritten;
        PathUtils.IsBasePathOverwritten = false;
        s_vertMaterial = Material.GetMaterialInstance(Shader.GetNewShaderInstance("sphereShader"));
        PathUtils.IsBasePathOverwritten = previousState;
        var _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferStorage(BufferTarget.ArrayBuffer, CubeData.VerticesSize, CubeData.Vertices, BufferStorageFlags.None);

        var _elementBufferObject = GL.GenBuffer();
        s_vao = GL.GenVertexArray();
        GL.BindVertexArray(s_vao);
        SetAttributes();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.BufferStorage(BufferTarget.ElementArrayBuffer, CubeData.IndicesSize, CubeData.Indices, BufferStorageFlags.None);

        GL.BindVertexArray(0);
    }

    private static void SetAttributes()
    {
        int positionLocation = s_vertMaterial.GetAttribLocation("aPos");
        GL.EnableVertexAttribArray(positionLocation);
        GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
    }

    public void DrawAt(Vector3 atPosition)
    {
        GL.BindVertexArray(s_vao);
        s_vertMaterial.SetMatrix4("model", Matrix4x4.CreateScale(0.15f) * Matrix4x4.CreateTranslation(atPosition));
        s_vertMaterial.SetMatrix4("view", LevelEditorCamera.View);
        s_vertMaterial.SetMatrix4("projection", LevelEditorCamera.Projection);
        s_vertMaterial.UseMaterial();
        GL.DrawElements(PrimitiveType.Triangles, CubeData.Indices.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    } 
}