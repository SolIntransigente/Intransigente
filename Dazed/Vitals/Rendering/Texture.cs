using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dazed.Vitals.Rendering
{
    public abstract class Texture : IDisposable
    {
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
    }
}
