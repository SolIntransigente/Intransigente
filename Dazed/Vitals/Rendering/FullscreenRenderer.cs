using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Dazed.Vitals.Rendering
{
    internal class FullscreenRenderer
    {
        private readonly static int _vertexBufferObject, _vertexArrayObject, _elementsBufferObject;
        readonly Shader _screenShader;
        private static readonly Shader DefaultShader;

        static FullscreenRenderer()
        {
            DefaultShader = Shader.GetShaderInstance("screenShader");
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, QuadData.VertexPosAndTexturePosData.Length * sizeof(float), QuadData.VertexPosAndTexturePosData, BufferUsageHint.StaticDraw);

            _elementsBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementsBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, QuadData.Indices.Length * sizeof(uint), QuadData.Indices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
        }

        public FullscreenRenderer(Shader? screenShader = null)
        {
            _screenShader = screenShader ?? DefaultShader;
            PositionAttributePointer();
        }

        private void PositionAttributePointer()
        {
            _screenShader.Use();
            GL.BindVertexArray(_vertexArrayObject);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        }

        public void Render(int textureId)
        {
            GL.BindVertexArray(_vertexArrayObject);
            _screenShader.Use();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            GL.DrawElements(PrimitiveType.Triangles, QuadData.Indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
    }
}
