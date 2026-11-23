using System;
using System.IO;
using System.Numerics;
using Dazed.Vitals;
using DazedGUITools.EditorCommons;
using DazedGUITools.Windows.LevelEditor.Representations;
using ImGuiNET;

namespace DazedGUITools.Windows.LevelEditor;

internal class LevelEditorPropertiesWindow() : WindowBaseClass("Object Properties", true)
{
    private string m_tempPath = "Set material path";
    private (bool?, string) m_materialSetState;
    private static FieldsDrawingHelper _drawingHelper = null!;

    protected override void OnInitialize()
    {
        _drawingHelper = new();
        base.OnInitialize();
    }
    protected override void Draw()
    {
        if (LevelEditorContext.CurrentObject == null)
        {
            ImGui.Text("No object selected");
            return;
        }
        bool drawnPrevious = false;
        if (LevelEditorContext.CurrentObject.ObjType.HasFlag(ObjectType.Geometry))
        {
            DrawAsGeometry();
            drawnPrevious = true;
        }
        if (LevelEditorContext.CurrentObject.ObjType.HasFlag(ObjectType.DynamicObject))
        {
            if (drawnPrevious)
                ImGui.Separator();
            DrawAsDynamic();
        }
    }

    private void DrawAsDynamic()
    {
        IDynamicRepresentation sDyn = (IDynamicRepresentation) LevelEditorContext.CurrentObject!;
        for (int i = 0; i < sDyn.Properties.Length; i++)
        {
            var val = DrawForProperty(sDyn.Properties[i], out bool changed);
            if (!changed)
                continue;
            sDyn.ApplyValue(i, val);
        }
    }


    private static object DrawForProperty(DynamicObjectProperty dynamicObjectProperty, out bool changed)
    {
        _drawingHelper.DrawFor(dynamicObjectProperty.ParameterName, dynamicObjectProperty.ValueType, ref dynamicObjectProperty.m_paramValue);
        changed = false;
        return null!;
    }

    private void DrawAsGeometry()
    {
        IGeometryRepresentation sGeom = (IGeometryRepresentation) LevelEditorContext.CurrentObject!;
        m_tempPath = Path.GetRelativePath(MainWindow.ProjectFolder, sGeom.MaterialPath);
        var currentVert = LevelEditorContext.GetCurrentVertex();
        var vertPos = currentVert!.Position;
        if (InputVector3("Vert Pos", ref vertPos))
            sGeom.SetPosition(LevelEditorContext.SelectedVertexIndex, vertPos);
        ImGui.Separator();
        if (m_materialSetState.Item1 != null)
            if (!m_materialSetState.Item1.Value)
                ImGui.TextColored(Vector4Colors.Red, m_materialSetState.Item2);
        if (ImGuiFile.InputFile("Material file", ref m_tempPath))
            if (!sGeom.TrySetMaterial(m_tempPath, out string message))
                m_materialSetState = (true, message);
        Representations.VertexData sVertex = sGeom.VertexAt(LevelEditorContext.SelectedVertexIndex);
        ImGui.Separator();
        DrawVertexData(sGeom, sVertex);
    }

    private static void DrawVertexData(IGeometryRepresentation sGeom, VertexData sVertex)
    {
        if (ImGui.Button("Swap Z parenting"))
        {
            sGeom.SwapZParenting();
        }
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