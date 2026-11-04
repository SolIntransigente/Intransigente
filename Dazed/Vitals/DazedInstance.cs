using System.ComponentModel;
using System.Diagnostics;
using Dazed.Vitals.AssetReference;
using Dazed.Vitals.Audio;
using Dazed.Vitals.Rendering;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Dazed.Vitals
{
    public class DazedInstance : GameWindow
    {
        public static Stopwatch LastUpdateCall { get; private set; } = new();
        /// <summary>
        /// True when the game is ran to be played
        /// </summary>
        internal static bool IsContextRuntime { get; private set; }

        public static bool HasQuit { get; private set; }

        private readonly static KeyCombination _reloadShadersCombo = new(Keys.LeftControl, Keys.F8);

        public DazedInstance(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            MaximumSize = new(CurrentMonitor.HorizontalResolution, CurrentMonitor.VerticalResolution);
            ClientSize = MaximumSize.Value;
            AlwaysOnTop = false;
            IsContextRuntime = true;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            Artist.Initialize();
            Artist.SetResolution(ClientSize.X, ClientSize.Y);
            
            UpdateFrequency = CurrentMonitor.CurrentVideoMode.RefreshRate;
            VSync = VSyncMode.Adaptive;

            SplashScreen.InitializeSplashScreen();
            Phone.SubscribeToDrawCall(SplashScreen.Draw);
            SplashScreen.OnEndReached += EndSplashScreen;
        }

        private void EndSplashScreen()
        {
            Phone.UnSubscribeToDrawCall(SplashScreen.Draw);
            SplashScreen.OnEndReached -= EndSplashScreen;
            //TODO: Load main menu / start of game
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Phone.Quit();
            HasQuit = true;
            LastUpdateCall.Stop();
            OmniSonus.Terminate();
            base.OnClosing(e);
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);
            Artist.SetResolution(e.Width, e.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            LastUpdateCall.Start();
            base.OnUpdateFrame(args);
            Chronos.UpdateDeltaTime(Convert.ToSingle(args.Time));
            Input.UpdateInputState(KeyboardState, MouseState);

#if DEBUG
            if (Input.IsKeyDown(Keys.F10))
                Console.Clear();

            if (_reloadShadersCombo.FirstHoldSecondPressed())
            {
                ProgramFuncs.RetrieveDataFiles();
                Artist.TriggerAllShadersReload();
            }
#else
            if (_reloadShadersCombo.FirstHoldSecondPressed())
                Artist.TriggerAllShadersReload();
#endif
            if (Input.KeyJustPressed(Keys.F11))
            {
                switch (WindowState)
                {
                    case WindowState.Fullscreen:
                        WindowState = WindowState.Maximized;
                        break;
                    default:
                        WindowState = WindowState.Fullscreen;
                        break;
                }
            }
            Phone.Update();
            LastUpdateCall.Restart();
        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            if(HasQuit)
                return;
            Phone.Draw();
            Artist.DrawFrame(args);
            SwapBuffers();
        }
    }
}
