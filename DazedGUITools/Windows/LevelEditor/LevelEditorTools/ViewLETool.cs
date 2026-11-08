using Dazed.Vitals;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DazedGUITools.Windows.LevelEditor.LevelEditorTools
{
    internal class ViewLETool : ILevelEditorTool
    {
        //private static float _rotX, _rotY;
        public string ToolName => "View";

        public void Initialize()
        {
        }

        public void Operate()
        {
            if (Input.IsKeyDown(Keys.I))
                LevelEditorViewport.RotationX -= Chronos.UnscaledDeltaTime * 45;
            if (Input.IsKeyDown(Keys.K))
                LevelEditorViewport.RotationX += Chronos.UnscaledDeltaTime * 45;
            if (Input.IsKeyDown(Keys.J))
                LevelEditorViewport.RotationY -= Chronos.UnscaledDeltaTime * 45;
            if (Input.IsKeyDown(Keys.L))
                LevelEditorViewport.RotationY += Chronos.UnscaledDeltaTime * 45;
        }

        public void Terminate()
        {
        }
    }
}
