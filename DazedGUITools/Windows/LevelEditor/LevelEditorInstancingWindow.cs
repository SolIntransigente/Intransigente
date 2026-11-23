using System;
using Dazed.Vitals.Paisaje;
using ImGuiNET;

namespace DazedGUITools.Windows.LevelEditor;

public class LevelEditorInstancingWindow() : WindowBaseClass("Instance Object", true)
{
    private string searchFilter = string.Empty;

    protected override void Draw()
    {
        ImGui.Text("Select an object to instantiate");
        ImGui.Separator();
        if (ImGui.Button("Close"))
            Close();
        ImGui.Separator();
        ImGui.InputText("Type filter", ref searchFilter, (uint)short.MaxValue);
        ImGui.Separator();

        for(int i = 0; i < LevelEditorContext.PaisajeObjectTypes.Length; i++)
        {
            var obj = LevelEditorContext.PaisajeObjectTypes[i];
            if (!string.IsNullOrEmpty(searchFilter) && !obj.Name.Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
                continue;
            if (ImGui.Button(obj.Name))
            {
                LevelEditorContext.CreateDynamicObject(GetTypeFor(obj), obj);
                Close();
            }
        }
    }

    private Type GetTypeFor(Type obj)
    {
        if (obj.IsSubclassOf(typeof(Entity)))
            return typeof(Entity);
        if (obj.IsSubclassOf(typeof(Digemet)))
            return typeof(Digemet);
        throw new NotImplementedException($"No base type is related to type: {obj.Name}");
    }
}