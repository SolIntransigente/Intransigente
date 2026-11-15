using System;
using System.Linq;
using Dazed.Vitals;
using DazedGUITools.Windows.LevelEditor.LevelEditorTools;
using DazedGUITools.Windows.LevelEditor.Representations;
using ImGuiNET;

namespace DazedGUITools.Windows.LevelEditor
{
    internal class LevelEditorToolsWindow : WindowBaseClass
    {
        public LevelEditorToolsWindow() : base("Level Editor Tools", true)
        {}

        protected override void Draw()
        {
            int[] ints = Input.GetPressedNumbers();
            int pressedNumber = ints.Length == 0 ? 0 : ints.First();
            int expectedNumber = 1;
            if (ImGui.Button("Reload all geometry data"))
                ReloadAllGeometry();
            ImGui.Separator();
            //TODO: Replace with icons
            if (ImGui.Button("View") || pressedNumber == expectedNumber++)
                LevelEditorContext.SetTool(new ViewLETool());
            if (ImGui.Button("Translate") || pressedNumber == expectedNumber++)
                LevelEditorContext.SetTool(new TranslateLETool());
        }

        private void ReloadAllGeometry()
        {
            GeometryRepresentation.CallReloadAll(this);
        }
    }
}
