using System.Numerics;
using ImGuiNET;

namespace DazedGUITools.Windows.LevelEditor;

internal class LevelEditorGeometryWindow() : WindowBaseClass("Geometry properties", true)
{
    protected override void Draw()
    {
        if (LevelEditorContext.SelectedGeometry == null)
        {
            ImGui.TextColored(Vector4Colors.Red, "No geometry selected!");
            return;
        }
        var currentVert = LevelEditorContext.GetCurrentVertex();
        var vertPos = currentVert!.Position;
        if(InputVector3("Vert Pos", ref vertPos))
            LevelEditorContext.SelectedGeometry.SetPosition(LevelEditorContext.SelectedVertexIndex, vertPos);
        ImGui.Separator();
        
    }
}