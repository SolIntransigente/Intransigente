using System;
using System.IO;
using Dazed.Vitals;
using Dazed.Vitals.Rendering;
using ImGuiNET;

namespace DazedGUITools.Windows.MaterialCreation;
[MainMenuWindow("Material Creation")]
public class MaterialCreationWindow() : WindowBaseClass("Material Creation", true)
{
    private string m_materialName = string.Empty, m_fragmentPath = string.Empty, m_vertexPath = string.Empty;
    private bool m_isLit;

    private bool? m_result;
    private string? m_resultMessage;
    protected override void Draw()
    {
        ImGui.Text("Insert the proper values to create a material data file");
        ImGui.Separator();
        ImGui.InputText("Material name", ref m_materialName, ushort.MaxValue);
        ImGui.Separator();
        if (ImGuiFile.InputFile("Fragment shader path", ref m_fragmentPath))
            m_fragmentPath = Path.GetRelativePath(MainWindow.ProjectShadersFolder, m_fragmentPath);
        if (ImGuiFile.InputFile("Vertex shader path", ref m_vertexPath))
            m_vertexPath = Path.GetRelativePath(MainWindow.ProjectShadersFolder, m_vertexPath);
        ImGui.Separator();
        ImGui.Checkbox("Is Lit?", ref m_isLit);
        ImGui.Separator();
        bool conditionsMet = ConditionsMet();
        if (!conditionsMet)
            ImGui.TextColored(Vector4Colors.Red, "One of the inserted values is not valid");
        if(m_result != null)
            if(m_result.Value)
                ImGui.TextColored(Vector4Colors.Green, "Material data created successfully.");
            else
            {
                ImGui.TextColored(Vector4Colors.Red, $"Material data failed at being created with the following code:");
                ImGui.TextColored(Vector4Colors.Red, m_resultMessage);
            
            }
        if(ImGui.Button("Create") && conditionsMet)
            CreateMaterialData();
    }

    private void CreateMaterialData()
    {
        try
        {
            string path = Path.Combine(MainWindow.ProjectResourcesFolder, CommonPaths.MaterialFolderName);

            var errorMsg = Shader.GetProperties(MainWindow.ProjectShadersFolder, m_fragmentPath, m_vertexPath, out SerializedMaterialProperty[] properties);

            if (errorMsg != null)
            {
                m_result = false;
                m_resultMessage = errorMsg;
                return;
            }

            SerializedMaterialData matData = new(m_materialName, Path.GetFileNameWithoutExtension(m_fragmentPath), Path.GetFileNameWithoutExtension(m_vertexPath), m_isLit, properties);
            Cereal.SaveData(matData, path, $"{m_materialName}.material");
            m_result = true;
        }
        catch(Exception ex)
        {
            Scriptorium.LogException(ex);
            m_result = false;
        }
    }

    private bool ConditionsMet()
    {
        if (m_materialName == string.Empty)
            return false;
        if (!PathUtils.IsValidPath(m_fragmentPath))
            return false;
        if (!PathUtils.IsValidPath(m_vertexPath))
            return false;
        return true;
    }
}