using System.Numerics;
using ImGuiNET;

namespace DazedGUITools.Windows.LevelEditor;

internal class LevelEditorGeometryWindow() : FileManagedWindow("Geometry properties", true)
{
    private string m_tempPath = "Set material path";
    private (bool?, string) m_materialSetState;
    protected override void OnInitialize()
    {
        base.OnInitialize();
    }
    protected override void Draw()
    {
        Representations.GeometryRepresentation sGeom = LevelEditorContext.SelectedGeometry;
        if (sGeom == null)
        {
            ImGui.TextColored(Vector4Colors.Red, "No geometry selected!");
            return;
        }
        var currentVert = LevelEditorContext.GetCurrentVertex();
        var vertPos = currentVert!.Position;
        if(InputVector3("Vert Pos", ref vertPos))
            sGeom.SetPosition(LevelEditorContext.SelectedVertexIndex, vertPos);
        ImGui.Separator();
        if (m_materialSetState.Item1 != null)
            if (!m_materialSetState.Item1.Value)
                ImGui.TextColored(Vector4Colors.Red, m_materialSetState.Item2);
        if (InputFile("Material file", ref m_tempPath)) 
            if (!sGeom.TrySetMaterial(m_tempPath, out string mesage))
                m_materialSetState = (true, mesage);
        Representations.VertexData sVertex = sGeom.VertexAt(LevelEditorContext.SelectedVertexIndex);
        if (!sVertex.IsAbsoluteVertex) 
        {
            if (ImGui.Checkbox("First parent X", ref sVertex.Flag.useX0))
                sGeom.ForceUpdate();
            if (ImGui.Checkbox("First parent Y", ref sVertex.Flag.useY0))
                sGeom.ForceUpdate();
            if (ImGui.Checkbox("First parent Z", ref sVertex.Flag.useZ0))
                sGeom.ForceUpdate();
        }
    }
}