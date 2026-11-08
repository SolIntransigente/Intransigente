using ImGuiNET;

namespace DazedGUITools.Windows.LevelEditor;

internal class LevelEditorSectionsWindows : WindowBaseClass
{
    public LevelEditorSectionsWindows() : base("Sections list", true)
    {
    }

    protected override void Draw()
    {
        ImGui.Text("Currently available sections:");
        ImGui.TextColored(Vector4Colors.Red, $"Currently selected section: {LevelEditorContext.CurrentlySelectedSection.SectionUniqueID}");
        ImGui.Separator();
        if (ImGui.Button("Create Section"))
            LevelEditorContext.CreateAndAddNewSection();
        int count = LevelEditorContext.AllSections.Count;
        for (ushort i = 0; i < count; i++)
        {
            string label = LevelEditorContext.AllSections[i].Visible ? "<o>" : "<->";
            if (ImGui.SmallButton(label + $"##{i}"))
                LevelEditorContext.AllSections[i].Visible = !LevelEditorContext.AllSections[i].Visible;
            ImGui.SameLine();
            if (ImGui.SmallButton($"Section: {LevelEditorContext.AllSections[i].SectionUniqueID}##{i}"))
                LevelEditorContext.SelectSection(i);
            if(i % 2f == 0 && i != count - 1)
            {
                ImGui.SameLine();
                ImGui.Text("|");
                ImGui.SameLine();
            }
        }
    }
}