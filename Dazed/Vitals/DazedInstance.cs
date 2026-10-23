using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using Dazed.Vitals.AssetReference;
using Dazed.Vitals.Audio;
using Dazed.Vitals.Paisaje;
using Dazed.Vitals.Rendering;
using Dazed.Vitals.Rendering.GUI;
using Dazed.Vitals.VideoPlay;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Dazed.Vitals
{
    public class DazedInstance : GameWindow
    {
        static Vector3 movement = new();
        static Quaternion debugViewRotation = new();

        public static Stopwatch LastUpdateCall { get; private set; } = new();

        private static ViewportCamera DebugViewportCamera;
        [ExposedField("defaultFont")]
        private AssetPath _defaultFont;
        [ExposedField("playerMesh")]
        private AssetPath _playerMeshPath;
        [ExposedField("testSound")]
        private AssetPath _testSoundPath;
        //private SoundEmitter _testEmitter;
        //static float rotY;
        //static Shader _testShader;
        //static Material _testMaterial;
        //static Mesh _testMesh;
        //static Transform _testMeshTransform;
        //static GuiButton _testButton;
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
            ExposedData.RetrieveAll(GetType(), this);
            Artist.Initialize();
            Artist.SetResolution(ClientSize.X, ClientSize.Y);
            
            UpdateFrequency = CurrentMonitor.CurrentVideoMode.RefreshRate;
            VSync = VSyncMode.Adaptive;

            SplashScreen.InitializeSplashScreen();
            Phone.SubscribeToDrawCall(SplashScreen.Draw);
            SplashScreen.OnEndReached += EndSplashScreen;

            //TEST DATA
            //_testMeshTransform = new();
            //Font.DefaultFont = Font.GetFontInstance(_defaultFont.AssetRelativePath);

            //DirectionalLight dirL = new(-Vector3.UnitZ, Color.White, 1);
            //_testButton = new("Test");
            //_testButton.Size = new(4, 2);
            //_testButton.TextRender.AddPosition(-0.15f, 0);
            //_testButton.OnButtonPress += () => { Scriptorium.Warn("hey"); };
            //_testShader = Shader.GetShaderInstance("default.vert", "default.frag");
            //_testMaterial = Material.GetMaterialInstance(_testShader, true);
            //_testMesh = Mesh.GetMeshInstance(_playerMeshPath.AssetRelativePath, _testMaterial);
            //DebugViewportCamera = new(90);

            //_testEmitter = new(_testSoundPath, SoundEmissionSettings.DefaultLooped);
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
            //movement = Vector3.Zero;
            //if (Input.IsKeyDown(Keys.W))
            //    movement.Z += -1;
            //if (Input.IsKeyDown(Keys.S))
            //    movement.Z += 1;
            //if (Input.IsKeyDown(Keys.D))
            //    movement.X += 1;
            //if (Input.IsKeyDown(Keys.A))
            //    movement.X += -1;
            //if (Input.IsKeyDown(Keys.E))
            //    movement.Y += 1;
            //if (Input.IsKeyDown(Keys.Q))
            //    movement.Y += -1;
            //DebugViewportCamera.Move(movement, Chronos.DeltaTime);

            //if (Input.IsKeyDown(Keys.Left))
            //    rotY += -Chronos.DeltaTime;
            //if (Input.IsKeyDown(Keys.Right))
            //    rotY += Chronos.DeltaTime;
            //debugViewRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, rotY);
            //DebugViewportCamera.Transform.Rotation = debugViewRotation;
#if DEBUG
            if (Input.IsKeyDown(Keys.F10))
                Console.Clear();

            //if (Input.IsKeyDown(Keys.I))
            //    _testMeshTransform.MoveGlobal(Vector3.UnitZ * Chronos.DeltaTime);
            //if (Input.IsKeyDown(Keys.J))
            //    _testMeshTransform.MoveGlobal(-Vector3.UnitX * Chronos.DeltaTime);
            //if (Input.IsKeyDown(Keys.K))
            //    _testMeshTransform.MoveGlobal(-Vector3.UnitZ * Chronos.DeltaTime);
            //if (Input.IsKeyDown(Keys.L))
            //    _testMeshTransform.MoveGlobal(Vector3.UnitX * Chronos.DeltaTime);
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
            //_testMesh.ApplyRelativeToTransform(_testMeshTransform.World);
            Artist.DrawFrame(args);
            SwapBuffers();
        }
    }
}
