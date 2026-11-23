using Dazed.Vitals.AssetReference;
using Dazed.Vitals.Rendering;
using Dazed.Vitals.Rendering.GUI;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Dazed.Vitals
{
    public class ExceptionThrownMessage : GameWindow
    {
        private string _messageContent;
        private GuiText _message;
        [ExposedField("defaultFont", ".fd (Font data)")]
        private static AssetPath fontAsset;
        private GuiButton _closeGameButton;
        public ExceptionThrownMessage(string message) : base(new(), new() { ClientSize = new(640, 480) })
        {
            _messageContent = message;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            Artist.Initialize();
            Artist.SetResolution(ClientSize.X, ClientSize.Y);

            fontAsset = new("defaultFont");
            Font.DefaultFont = Font.GetFontInstance(fontAsset, true);

            _message = new(_messageContent)
            {
                ScreenPosition = new(-0.8f, -0.75f),
                CharactersPerLine = 40,
                CharacterSize = 25
            };
            _closeGameButton = new("CLOSE")
            {
                ScreenPosition = new(0, 0.25f)
            };
            _closeGameButton.OnButtonPress += Close;

            Shader.Reload();
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);
        }

        protected override void OnMove(WindowPositionEventArgs e)
        {
            base.OnMove(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            Input.UpdateInputState(KeyboardState, MouseState);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            GL.ClearColor(Color.Red);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            _message.Render(default);
            _closeGameButton.Render(default);

            SwapBuffers();
        }
    }
}
