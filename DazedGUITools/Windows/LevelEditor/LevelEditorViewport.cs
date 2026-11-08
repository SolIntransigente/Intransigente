using Dazed.Vitals;
using ImGuiNET;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Numerics;

namespace DazedGUITools.Windows.LevelEditor
{
    [MainMenuWindow("Level editor viewport")]
    internal class LevelEditorViewport() : WindowBaseClass("Level editor", true)
    {
        public static float RotationX
        {
            get => _rotX;
            set => _rotX = value;
        }
        public static float RotationY
        {
            get => _rotY;
            set => _rotY = value;
        }
        private static float _rotX, _rotY;
        private static readonly Vector2 StartingSize = new Vector2(256, 256);

        private static LevelEditorToolsWindow s_toolsWindow = null!;
        private static LevelEditorSectionsWindows s_sectionsWindow = null!;
        public static float AspectRatio { get; internal set; } = 1;

        private static readonly KeyCombination PreviousVertexCombo = new (Keys.LeftControl, Keys.Space);

        protected override void OnInitialize()
        {
            LevelEditorContext.Initialize((short)StartingSize.X, (short)StartingSize.Y);
            s_toolsWindow = new LevelEditorToolsWindow();
            s_sectionsWindow = new LevelEditorSectionsWindows();
        }
        protected override void Draw()
        {
            if (ImGui.Button("Close"))
            {
                Close();
                s_toolsWindow.Close();
                s_sectionsWindow.Close();
            }
            if (ImGui.Button("Create quad"))
            {
                Vector3 min = new(LevelEditorCamera.Position.X - 1, LevelEditorCamera.Position.Y - 1, LevelEditorCamera.Position.Z);
                Vector3 max = new(LevelEditorCamera.Position.X + 1, LevelEditorCamera.Position.Y + 1, LevelEditorCamera.Position.Z);
                min = Vector3.Round(min);
                max = Vector3.Round(max);
                LevelEditorContext.AddNewQuad(min, max);
            }
            ImGui.SameLine();
            ImGui.Text(LevelEditorCamera.Position.ToString());
            Vector2 size = ImGui.GetWindowSize();
            LevelEditorContext.TryUpdateSize((short)size.X, (short)size.Y);
            LevelEditorContext.Draw();
            if (LevelEditorContext.Height > 0 && LevelEditorContext.Width > 0)
                AspectRatio = LevelEditorContext.Width / (float)LevelEditorContext.Height;
            ImGui.Image(LevelEditorContext.FrameBufferTexture, new(LevelEditorContext.Width, LevelEditorContext.Height));
            ProcessInput();
        }

        private static void ProcessInput()
        {
            ProcessCommands();

            if (Input.IsKeyDown(Keys.W))
                LevelEditorCamera.AddPositionRelative(Vector3.UnitZ, Chronos.DeltaTime * 5);
            if (Input.IsKeyDown(Keys.S))
                LevelEditorCamera.AddPositionRelative(-Vector3.UnitZ, Chronos.DeltaTime * 5);
            if (Input.IsKeyDown(Keys.D))
                LevelEditorCamera.AddPositionRelative(Vector3.UnitX, Chronos.DeltaTime * 5);
            if (Input.IsKeyDown(Keys.A))
                LevelEditorCamera.AddPositionRelative(-Vector3.UnitX, Chronos.DeltaTime * 5);
            if (Input.IsKeyDown(Keys.E))
                LevelEditorCamera.AddPosition(-Vector3.UnitY, Chronos.DeltaTime * 5);
            if (Input.IsKeyDown(Keys.Q))
                LevelEditorCamera.AddPosition(Vector3.UnitY, Chronos.DeltaTime * 5);

            if (Input.IsKeyDown(Keys.R))
            {
                LevelEditorCamera.Position = Vector3.Zero;
                LevelEditorCamera.Rotation = Quaternion.Identity;
                _rotX = 0;
                _rotY = 0;
            }
            if (Input.IsKeyDown(MouseButton.Middle))
            {
                _rotX -= Input.MouseDelta.Y * 0.1f;
                _rotY -= Input.MouseDelta.X * 0.1f;
            }
            if (Input.IsKeyDown(MouseButton.Right))
            {
                _rotX += Input.MouseDelta.Y * 0.1f;
                _rotY += Input.MouseDelta.X * 0.1f;
            }
            _rotX = MathF.Max(-89, MathF.Min(89, _rotX));
            LevelEditorCamera.SetEulerRotation(new(_rotX, _rotY, 0));
        }

        private static void ProcessCommands()
        {
            if(PreviousVertexCombo.FirstHoldSecondPressed())
                LevelEditorContext.ChangeVertex(false);
            else if(Input.KeyJustPressed(Keys.Space))
                LevelEditorContext.ChangeVertex();
        }
    }
}
