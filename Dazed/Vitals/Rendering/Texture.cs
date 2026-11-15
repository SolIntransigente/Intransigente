using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dazed.Vitals.Rendering
{
    public abstract class Texture : IDisposable
    {
        public abstract TextureType TextureType {get;}
        public TextureTarget Target => target;
        readonly TextureTarget target;
        internal readonly int _textureHandle;

        internal Texture(TextureTarget target)
        {
            this.target = target;
            _textureHandle = GL.GenTexture();
        }

        internal abstract void Use(TextureUnit unit = TextureUnit.Texture0);

        public void Dispose()
        {
            GL.DeleteTexture(_textureHandle);
        }

        internal static Texture GetTextureFor(TextureIdentification identification)
        {
            return identification.TextureType switch
            {
                TextureType.Texture2D => new Texture2D(identification.Path, identification.ImageDataType, identification.IsReadOnly),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
