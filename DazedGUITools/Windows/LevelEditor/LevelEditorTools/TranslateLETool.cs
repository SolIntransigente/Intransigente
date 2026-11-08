using Dazed.Vitals;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Numerics;

namespace DazedGUITools.Windows.LevelEditor.LevelEditorTools
{
    internal class TranslateLETool : ILevelEditorTool
    {
        public string ToolName => "Translation";

        public void Initialize()
        {
        }

        public void Operate()
        {
            bool shiftDown = Input.IsKeyDown(Keys.LeftShift);
            if (Input.KeyJustPressed(Keys.U))
                AddPos(shiftDown, -Vector3.UnitX);
            if (Input.KeyJustPressed(Keys.J))
                AddPos(shiftDown, Vector3.UnitX);

            if (Input.KeyJustPressed(Keys.I))
                AddPos(shiftDown, -Vector3.UnitY);
            if (Input.KeyJustPressed(Keys.K))
                AddPos(shiftDown, Vector3.UnitY);

            if (Input.KeyJustPressed(Keys.O))
                AddPos(shiftDown, Vector3.UnitZ);
            if (Input.KeyJustPressed(Keys.L))
                AddPos(shiftDown, -Vector3.UnitZ);
        }

        private static void AddPos(bool shiftDown, Vector3 unit)
        {
            if (!shiftDown)
                LevelEditorContext.AddSelectedVertexPosition(unit);
            else
                LevelEditorContext.AddGeometryPosition(unit);
        }

        public void Terminate()
        {
        }
    }
}
