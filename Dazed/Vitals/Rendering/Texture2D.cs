using OpenTK.Graphics.OpenGL;
using Dazed.Vitals.Content;
using StbImageSharp;
using System.Buffers;

namespace Dazed.Vitals.Rendering
{
    public class Texture2D : Texture
    {
        public ImageResult? TextureImage
        {
            get
            {
                return _isReadOnly ? null : _image;
            }
            set
            {
                if (_isReadOnly)
                {
                    if(!DebugState.DebugLow)
                        return;
                    Scriptorium.Warn("Image is readonly and can not be modified");
                    return;
                }
                if (value == null)
                    return;
                _image = value;
                PushData(_image);
            }
        }
        public float Width => _width;
        private float _width;
        public float Height => _height;
        private float _height;

        public ImageDataType ImageDataType => _dataType;
        private ImageDataType _dataType;
        private byte[] _pixelsBytes = null!;

        private ImageResult? _image;
        private readonly bool _isReadOnly;

        private static readonly Dictionary<TextureIdentification, Texture2D> InstantiatedTextures = new();
        public static Texture2D GetTextureInstance(TextureIdentification identification)
        {
            if(InstantiatedTextures.TryGetValue(identification, out var texture))
                return texture;
            texture = new(identification.Path, identification.ImageDataType, identification.IsReadOnly);
            InstantiatedTextures.Add(identification, texture);
            return texture;
        }
        public static Texture2D GetTextureInstance(string path) => GetTextureInstance(new TextureIdentification(path, ImageDataType.RGB));

        internal Texture2D(string path, ImageDataType dataType, bool isReadOnly)
            : base(TextureTarget.Texture2D)
        {
            _dataType = dataType;
            _isReadOnly = isReadOnly;
            using (var stream = File.OpenRead(path))
            {
                var img = ImageResult.FromStream(stream, GetColorComponent());
                PushData(img);
                if (!_isReadOnly)
                    _image = img;
                stream.Dispose();
            }
        }

        private ColorComponents GetColorComponent()
        {
            return _dataType switch
            {
                ImageDataType.RGB => ColorComponents.RedGreenBlue,
                ImageDataType.RGBA => ColorComponents.RedGreenBlueAlpha,
                ImageDataType.Grey => ColorComponents.Grey,
                _ => throw new NotImplementedException()
            };
        }

        private void UpdateByteArray(ref byte[] data)
        {
            if (_isReadOnly)
                return;
            _pixelsBytes = data;
        }

        private void PushData(ImageResult image)
        {
            _height = image.Height;
            _width = image.Width;
            byte[] data = ArrayPool<byte>.Shared.Rent(image.Data.Length);
            try
            {
                UpdateByteArray(ref data);

                GL.BindTexture(TextureTarget.Texture2D, _textureHandle);
                GL.TexImage2D(TextureTarget.Texture2D, 0, TranslatePixelInternalFormat(), image.Width, image.Height, 0, TranslatePixelFormat(), PixelType.UnsignedByte, image.Data);
                GL.TextureParameter(_textureHandle, TextureParameterName.GenerateMipmap, 1/*true*/);
                GL.TextureParameter(_textureHandle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TextureParameter(_textureHandle, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(data, true);
            }
        }
        public PixelInternalFormat TranslatePixelInternalFormat()
        {
            return _dataType switch
            {
                ImageDataType.RGB => PixelInternalFormat.R3G3B2,
                ImageDataType.RGBA => PixelInternalFormat.Rgba2,
                ImageDataType.Grey => PixelInternalFormat.Intensity4,
            };
        }
        public PixelFormat TranslatePixelFormat()
        {
            return _dataType switch
            {
                ImageDataType.RGB => PixelFormat.Rgb,
                ImageDataType.RGBA => PixelFormat.Rgba,
                ImageDataType.Grey => PixelFormat.Rgb,
            };
        }
        internal override void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, _textureHandle);
        }
    }
}
