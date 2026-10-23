using Dazed.Vitals.AssetReference;
using System.Numerics;

namespace Dazed.Vitals.Rendering.GUI
{
    public abstract class GuiElement : Renderable
    {
        public override RenderingPass RenderPass => RenderingPass.userInterface;

        public Vector2 ScreenPosition
        {
            get => _pos;
            set
            {
                _pos = value;
                ReloadWorldMatrix();
            }
        }
        private Vector2 _pos;

        public virtual Vector2 Size
        {
            get => _scl;
            set
            {
                _scl = value;
                ReloadWorldMatrix();
            }
        }
        protected Vector2 InternalScale { get; private set; }
        private Vector2 _scl = Vector2.One;

        protected Matrix4x4 World => _world;
        protected Matrix4x4 ScaleMatrix => _scaleMatrix;
        private Matrix4x4 _world, _scaleMatrix;

        public override bool AllowRendering => _allowRendering;
        protected bool _allowRendering = true;

        public void CanBeRendered(bool state) => _allowRendering = state;

        public GuiElement()
        {
            ReloadWorldMatrix();
            Artist.OnWidowResize += OnFrameBufferResize;
        }

        protected virtual void OnFrameBufferResize() 
        {
            ReloadWorldMatrix();            
        }

        public override void Dispose()
        {
            base.Dispose();
            Artist.OnWidowResize -= OnFrameBufferResize;
        }

        public void SetPositionInPixels(int x, int y)
        {
            x *= 2;
            y *= 2;
            ScreenPosition = new((x * Artist.NormalizedWidth) - 1f, (y * Artist.NormalizedHeight) - 1f);
        }

        public void AddPosition(float x, float y)
        {
            x += ScreenPosition.X;
            y += ScreenPosition.Y;
            ScreenPosition = new(x, y);
        }

        public void SetPositionScreenRelative(float x, float y)
        {
            x *= 2;
            y *= 2;
            ScreenPosition = new(x - 1f, y - 1f);
        }

        protected virtual void ReloadWorldMatrix()
        {
            float desiredX = _scl.X;
            float desiredY = _scl.Y;
            if (_scl.X * Artist.Width > _scl.Y * Artist.Height)
                desiredX = _scl.X * Artist.AspectRatio;
            else if (_scl.X * Artist.Width < _scl.Y * Artist.AspectRatio)
                desiredY = _scl.Y * Artist.AspectRatio;
            InternalScale = new(desiredX * 0.1f, desiredY * 0.1f);
            _scaleMatrix = Matrix4x4.CreateScale(InternalScale.X, InternalScale.Y, 1);
            _world = _scaleMatrix * Matrix4x4.CreateTranslation(new(_pos.X, _pos.Y, 0));
        }
    }
}
