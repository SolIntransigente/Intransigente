using DazedGUITools.Windows.LevelEditor.LevelEditorTools;
using ImGuiNET;

namespace DazedGUITools.Windows.LevelEditor
{
    internal class LevelEditorToolsWindow : WindowBaseClass
    {
        public LevelEditorToolsWindow() : base("Level Editor Tools", true)
        {}

        protected override void Draw()
        {
            //TODO: Replace with icons
            //TODO: Find and list tools at startup
            if (ImGui.Button("View"))
                LevelEditorContext.SetTool(new ViewLETool());
            if (ImGui.Button("Translate"))
                LevelEditorContext.SetTool(new TranslateLETool());
        }
    }
}
