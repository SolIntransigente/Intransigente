using System.IO;
using System.Numerics;
using DazedGUITools.Windows.LevelEditor.Representations;
using ImGuiNET;

namespace DazedGUITools.Windows.LevelEditor;

internal class LevelEditorGeometryWindow() : WindowBaseClass("Geometry properties", true)
{
    private string m_tempPath = "Set material path";
    private (bool?, string) m_materialSetState;
    protected override void OnInitialize()
    {
        base.OnInitialize();
    }
    protected override void Draw()
    {
        GeometryRepresentation? sGeom = LevelEditorContext.SelectedGeometry;
        if (sGeom == null)
        {
            ImGui.TextColored(Vector4Colors.Red, "No geometry selected!");
            return;
        }
        m_tempPath = Path.GetRelativePath(MainWindow.ProjectFolder, sGeom.m_materialPath);
        var currentVert = LevelEditorContext.GetCurrentVertex();
        var vertPos = currentVert!.Position;
        if(InputVector3("Vert Pos", ref vertPos))
            sGeom.SetPosition(LevelEditorContext.SelectedVertexIndex, vertPos);
        ImGui.Separator();
        if (m_materialSetState.Item1 != null)
            if (!m_materialSetState.Item1.Value)
                ImGui.TextColored(Vector4Colors.Red, m_materialSetState.Item2);
        if (ImGuiFile.InputFile("Material file", ref m_tempPath)) 
            if (!sGeom.TrySetMaterial(m_tempPath, out string mesage))
                m_materialSetState = (true, mesage);
        Representations.VertexData sVertex = sGeom.VertexAt(LevelEditorContext.SelectedVertexIndex);
        ImGui.Separator();
        DrawVertexData(sGeom, sVertex);
        
    }

    private static void DrawVertexData(GeometryRepresentation sGeom, VertexData sVertex)
    {
        if (ImGui.Button("Swap Z parenting"))
        {
            sGeom.SwapZParenting();
        }
        // if (!sVertex.IsAbsoluteVertex)
        // {
        //     if (ImGui.Checkbox("Which vertex Z", ref sVertex.Flag.useZ0))
        //         sGeom.ForceUpdate();
        //     ImGui.Text("Whether it should use the Z coordinate of the first vertex it references or the other");
        //     ImGui.Separator();
        // }
        bool useZToY = sGeom.UseZToYTextureCoordinate;
        if (ImGui.Checkbox("Use Z for texture coordinate Y", ref useZToY))
        {
            sGeom.UseZToYTextureCoordinate = useZToY;
        }
        bool useZToX = sGeom.UseZToXTextureCoordinate;
        if (ImGui.Checkbox("Use Z for texture coordinate X", ref useZToX))
        {
            sGeom.UseZToXTextureCoordinate = useZToX;
        }
        bool flipX = sGeom.FlipTextureCoordinateX;
        if (ImGui.Checkbox("Flip X texture coordinate", ref flipX))
        {
            sGeom.FlipTextureCoordinateX = flipX;
        }
        bool flipY = sGeom.FlipTextureCoordinateY;
        if (ImGui.Checkbox("Flip Y texture coordinate", ref flipY))
        {
            sGeom.FlipTextureCoordinateY = flipY;
        }
    }
}