using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Dazed.Vitals.Rendering.GUI
{
    internal class ImageGui : GuiElement
    {
        public Color ImageColor
        {
            get => _imgColor; 
            set => _imgColor = value;
        }
        private Color _imgColor = Color.White;

        public Texture2D Image
        {
            get => _image;
            set => _image = value;
        }
        private Texture2D _image;

        public bool Transparent
        {
            get => _transparent;
            set
            {
                _transparent = value;
                _imageShader = DefaultShader;
            }
        }
        private bool _transparent;

        public Shader? ImageShader
        {
            get => _imageShader;
            set => _imageShader = value ?? DefaultShader;
        }

        private Shader _imageShader;

        private static readonly int Vao;
        private static readonly Shader DefaultShader;

        static ImageGui()
        {
            DefaultShader = Shader.GetNewShaderInstance("guiImage");

            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferStorage(BufferTarget.ArrayBuffer, QuadData.VertexPosAndTexturePosData.Length * sizeof(float), QuadData.VertexPosAndTexturePosData, BufferStorageFlags.None);
            //GL.BufferData(BufferTarget.ArrayBuffer, QuadData.VertexPosAndTexturePosData.Length * sizeof(float), QuadData.VertexPosAndTexturePosData, BufferUsageHint.StaticDraw);

            int ebo = GL.GenBuffer();
            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferStorage(BufferTarget.ElementArrayBuffer, QuadData.Indices.Length * sizeof(uint), QuadData.Indices, BufferStorageFlags.None);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, QuadData.Indices.Length * sizeof(uint), QuadData.Indices, BufferUsageHint.StaticDraw);

            int positionLocation = DefaultShader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            int textureCoordLocation = DefaultShader.GetAttribLocation("aTexCoords");
            GL.EnableVertexAttribArray(textureCoordLocation);
            GL.VertexAttribPointer(textureCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            GL.BindVertexArray(0);
        }

        public ImageGui(Texture2D? tex, Shader? shader = null) : base()
        {
            _imageShader = shader ?? DefaultShader;
            _image = tex ?? Artist.MissingTexture;
        }

        private void SetAllShaderValues()
        {
            _imageShader.SetMatrix4("world", World);
            _imageShader.SetColor4("color", _imgColor);
            _imageShader.SetBool("transparent", _transparent);
        }

        public override void Render(in RenderingData data)
        {
            GL.BindVertexArray(Vao);
            _imageShader.Use();
            _image.Use();
            SetAllShaderValues();
            GL.DrawElements(PrimitiveType.Triangles, QuadData.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }
    }
}
