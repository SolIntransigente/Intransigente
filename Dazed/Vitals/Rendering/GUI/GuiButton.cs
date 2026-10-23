using OpenTK.Mathematics;

namespace Dazed.Vitals.Rendering.GUI
{
    internal class GuiButton : ImageGui
    {
        public GuiText TextRender => _textRender;
        private readonly GuiText _textRender;

        public event Action? OnButtonPress, OnButtonRelease, OnButtonHover, OnButtonLostHover;
        public bool IsPressed => _isPressed;
        public bool IsHovered => _isHovered;
        private bool _isPressed, _isHovered;

        public string Text { get => _textRender.Text; set => _textRender.Text = value; }

        private float _x0;
        private float _y0;
        private float _x1;
        private float _y1;

        public GuiButton(string? text = null, Texture2D? tex = null, Shader? shader = null) : base(tex, shader)
        {
            _textRender = new(text);
            _textRender.CanBeRendered(false);
        }

        public override void Render(in RenderingData data)
        {
            base.Render(data);
            CheckPress();
            _textRender.Render(data);
        }

        protected override void ReloadWorldMatrix()
        {
            base.ReloadWorldMatrix();
            _textRender?.ScreenPosition = new(TextRender.ScreenPosition.X, ScreenPosition.Y);
            ReloadData();
        }

        protected override void OnFrameBufferResize()
        {
            base.OnFrameBufferResize();
            ReloadData();
        }

        private void ReloadData()
        {
            float width = InternalScale.X * 0.5f;
            float height = InternalScale.Y * 0.5f;
            float posX = 0.5f * (ScreenPosition.X + 1);
            float posY = 0.5f * (ScreenPosition.Y + 1);

            _x0 = (posX - width);
            _y0 = (posY - height);
            _x1 = (posX + width);
            _y1 = (posY + height);
        }

        private void CheckPress()
        {
            Vector2 mousePos = Input.RelativeMousePosition;
            if (mousePos.X > _x1 || mousePos.X < _x0 || mousePos.Y > _y1 || mousePos.Y < _y0)
            {
                if (_isHovered)
                    OnButtonLostHover?.Invoke();
                _isHovered = false;
                return;
            }
            if (!_isHovered)
                OnButtonHover?.Invoke();
            _isHovered = true;
            if (!Input.LeftClickDown())
            {
                if (_isPressed)
                    OnButtonRelease?.Invoke();
                _isPressed = false;
                return;
            }
            if (_isPressed)
                return;
            _isPressed = true;
            OnButtonPress?.Invoke();
        }
}
}
