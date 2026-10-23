using OpenTK.Graphics.OpenGL;
using System.Numerics;

namespace Dazed.Vitals.Rendering.GUI
{
    public class GuiText : GuiElement
    {
        public Color Color
        {
            get => _textColor;
            set => _textColor = value;
        }
        private Color _textColor = Color.White;

        private Matrix4x4 _charScale;

        public Font? Font
        {
            get => _textFont;
            set => _textFont = value;
        }
        private Font? _textFont;

        private static readonly Shader TextShader;
        public float HorizontalSpacing
        {
            get => _horizontalSpacing;
            set
            {
                _horizontalSpacing = value;
                ReloadText(false);
            }
        }
        private float _horizontalSpacing = 0.1f;

        public float VerticalSpacing
        {
            get => _verticalSpacing;
            set
            {
                _verticalSpacing = value;
                ReloadText(false);
            }
        }
        private float _verticalSpacing = 0.25f;

        public ushort CharactersPerLine
        {
            get => _charPerLine;
            set
            {
                if (value <= 0)
                    return;
                _charPerLine = value;
                ReloadText(false);
            }
        }
        private ushort _charPerLine = 100;

        /// <summary>
        /// Default is 12
        /// </summary>
        public float CharacterSize
        {
            get => _size;
            set
            {
                if (value <= 0)
                    _size = 0.1f;
                else
                    _size = value;
                ReloadText(false);
            }
        }
        private float _size = 12;

        public string Text
        {
            get => _text;
            set
            {
                if (value == null)
                    _text = string.Empty;
                else
                    _text = value;
                ReloadText();
            }
        }

        private string _text;

        private Matrix4x4[] _charMatrix;
        private char[] _charValues;

        private static readonly int Vao;

        static GuiText()
        {
            TextShader = Shader.GetShaderInstance("text");

            var vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, QuadData.VertexPosAndTexturePosData.Length * sizeof(float), QuadData.VertexPosAndTexturePosData, BufferUsageHint.StaticDraw);

            var ebo = GL.GenBuffer();
            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, QuadData.Indices.Length * sizeof(uint), QuadData.Indices, BufferUsageHint.StaticDraw);

            int positionLocation = TextShader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            int textureCoordLocation = TextShader.GetAttribLocation("aTexCoords");
            GL.EnableVertexAttribArray(textureCoordLocation);
            GL.VertexAttribPointer(textureCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            GL.BindVertexArray(0);
        }

        private void ReloadText(bool reloadContent = true)
        {
            if (_text == string.Empty)
                return;

            TryReloadScaleMatrix();
            ReloadTextPositionAndContent(reloadContent);
        }

        private void ReloadTextPositionAndContent(bool reloadContent)
        {
            //_charPositions = new Vector2[_text.Length];
            _charMatrix = new Matrix4x4[_text.Length];
            if (reloadContent)
                _charValues = new char[_text.Length];
            short offsetX = 0;
            short offsetY = 0;
            short accumulated = 0;
            for (short i = 0; i < _text.Length; i++)
            {
                if (reloadContent)
                    _charValues[i] = _text[i];
                short line = (short)MathF.Round(i / _charPerLine);
                if (_charValues[i] == '\n')
                {
                    offsetY++;
                    offsetX = (short)(i + 1);
                    accumulated = 0;
                    continue;
                }
                if (_charValues[i] == ' ')
                    continue;
                if (accumulated != 0 && accumulated % _charPerLine == 0)
                {
                    offsetY++;
                    offsetX = i;
                }
                float x = (i - offsetX) * (_horizontalSpacing * _size * InternalScale.X);
                float y = (0 - offsetY * (_verticalSpacing * _size * InternalScale.Y));
                accumulated++;

                _charMatrix[i] = Matrix4x4.CreateTranslation(x + ScreenPosition.X, y - ScreenPosition.Y, 0);
            }
        }

        private float _lastSize;

        public GuiText(string? text) : base()
        {
            _text = text ?? string.Empty;
            ReloadText();
        }

        private void TryReloadScaleMatrix()
        {
            if (_lastSize == _size)
                return;

            ReloadWorldMatrix();
        }
        protected override void OnFrameBufferResize()
        {
            base.OnFrameBufferResize();
            ReloadTextPositionAndContent(false);
        }
        protected override void ReloadWorldMatrix()
        {
            base.ReloadWorldMatrix();
            if (_textFont == null)
            {
                if (DebugState.DebugMedium)
                    Scriptorium.Warn("Text renderer without font assigned. Setting default font.");
                _textFont = Font.DefaultFont;
                return;
            }
            ReloadTextPositionAndContent(_charValues == null);

            float scaleX = (_size) * _textFont.TextureCharScale.X;
            float scaleY = (_size) * _textFont.TextureCharScale.Y;
            _charScale = Matrix4x4.CreateScale(scaleX * InternalScale.X, scaleY * InternalScale.Y, 1);

            _lastSize = _size;
        }

        public override void Render(in RenderingData data)
        {
            if (_textFont == null)
            {
                if (DebugState.DebugMedium)
                    Scriptorium.Warn("Text renderer without font assigned. Setting default font.");
                _textFont = Font.DefaultFont;
                return;
            }

            GL.BindVertexArray(Vao);
            TextShader.Use();
            _textFont.FontTexture.Use();
            TextShader.SetVector2("texCoordsScale", _textFont.TextureCharScale);
            TextShader.SetVector4("textColorMultiplier", _textColor.ToVector4());

            for(int i = 0; i < _text.Length; i++)
            {
                if (_text[i] == ' ')
                    continue;
                if (_text[i] == '\n')
                    continue;
                Matrix4x4 charMatrix = _charScale * _charMatrix[i];
                TextShader.SetMatrix4("world", charMatrix);
                TextShader.SetVector2("texCoordsShift", _textFont.GetCharPosInAtlas(_text[i], out var sp));
                GL.DrawElements(BeginMode.Triangles, QuadData.Indices.Length, DrawElementsType.UnsignedInt, 0);
                if(sp != null)
                {
                    TextShader.SetVector2("texCoordsShift", _textFont.GetCharPosInAtlas(sp.Value, out var _));
                    GL.DrawElements(BeginMode.Triangles, QuadData.Indices.Length, DrawElementsType.UnsignedInt, 0);
                }
            }
            GL.BindVertexArray(0);
        }
    }
}
