using System;
using System.Collections.Generic;
using System.Text;

namespace Dazed.Vitals.Rendering
{
    public abstract class Renderable : IDisposable
    {
        public abstract bool AllowRendering { get; }
        protected virtual bool CustomAssignment => false;
        public virtual RenderingPass RenderPass => RenderingPass.geometryFirst;
        public abstract void Render(in RenderingData data);

        private readonly int _bindPos;

        public virtual void Dispose()
        {
            if (CustomAssignment) return;
            Artist.DisposeRendered(_bindPos);
            
            GC.SuppressFinalize(this);
        }

        internal Renderable()
        {
            if(!CustomAssignment)
                _bindPos = Artist.AssignRendered(this);
        }
    }
}
