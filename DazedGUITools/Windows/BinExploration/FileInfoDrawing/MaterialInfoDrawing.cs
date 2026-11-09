using Dazed.Vitals;
using Dazed.Vitals.Rendering;
using ImGuiNET;

namespace DazedGUITools.Windows.BinExploration.FileInfoDrawing;

public class MaterialInfoDrawing : FileInfoDrawingGeneric<SerializedMaterialData>
{
    public override string FilePath { get; protected set; } = null!;
    protected override SerializedMaterialData FileData { get; set; }
    
    private string[] m_uniforms;
    private object[] m_values;

    private bool m_isDirty;

    public override void Draw()
    {
        if (FileData.Equals(default))
        {
            ImGui.Text("No data exists");
            ImGui.Text($"Path: {FilePath}");
            return;
        }

    }

    public override void Initialize(string filePath)
    {
        FilePath = filePath;
        FileData = Cereal.LoadData<SerializedMaterialData>(filePath);
    }
}