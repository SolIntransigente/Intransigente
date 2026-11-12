using Dazed.Vitals;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Text;

namespace DazedGUITools.Windows
{
    public static class ImGuiFile
    {
        public static void PathValidMessage(string path, out bool pathIsCompatible)
        {
            pathIsCompatible = PathUtils.IsValidPath(path);
            if (!pathIsCompatible)
                ImGui.TextColored(Vector4Colors.Red, "Please enter a proper path!");
        }

        public static bool InputFile(string label, ref string result)
        {
            bool modified = false;
            if (ImGui.Button("Search##"+label))
            {
                result = FileSearching.OpenFileSearch();
                modified = true;
            }
            ImGui.SameLine();
            ImGui.InputText(label, ref result, 1024);
            return modified;
        }

        public static bool InputDirectory(string label, ref string result)
        {
            bool modified = false;
            if (ImGui.Button("Search##"+label))
            {
                result = FileSearching.OpenFolderSearch();
                modified = true;
            }
            ImGui.SameLine();
            ImGui.InputText(label, ref result, 1024);
            return modified;
        }
    }
}
