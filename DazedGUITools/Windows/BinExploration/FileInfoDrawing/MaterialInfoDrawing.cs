using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Dazed.Vitals;
using Dazed.Vitals.AssetReference;
using Dazed.Vitals.Rendering;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DazedGUITools.Windows.BinExploration.FileInfoDrawing;

public class MaterialInfoDrawing : FileInfoDrawingGeneric<SerializedMaterialData>
{
    public override string FilePath { get; protected set; } = null!;
    protected override SerializedMaterialData FileData { get; set; }
    
    private bool m_isLit;

    private string[] m_uniforms = null!;
    private Type[] m_types = null!;
    private object?[] m_values = null!;

    private bool m_isDirty;
    private bool? m_saveState;

    protected delegate void PropertyDrawMethod(int index, out bool wasChanged);
    
    private readonly Dictionary<Type, PropertyDrawMethod> m_typeToDrawMethod = new();
    private static string CurrentSetProperty = string.Empty;

    public override void Draw()
    {
        if (FileData.Equals(default))
        {
            ImGui.Text("No data exists");
            ImGui.Text($"Path: {FilePath}");
            return;
        }
        ImGui.Text($"For material: {FileData.MaterialName}");
        ImGui.Text($"Using fragment shader: {FileData.FragmentShaderRelativePath}");
        ImGui.Text($"Using vertex shader: {FileData.VertexShaderRelativePath}");
        if (m_isDirty)
            ImGui.TextColored(Vector4Colors.Red, "THERE ARE UNSAVED CHANGES");
        ImGui.Separator();
        if (ImGui.Checkbox("Is material lit?", ref m_isLit))
            m_isDirty = true;
        ImGui.Separator();
        for(int i = 0; i < FileData.Properties.Length; i++)
            RenderProperty(i);
        ImGui.Separator();
        if (m_saveState.HasValue)
            if (!m_saveState.Value)
                ImGui.TextColored(Vector4Colors.Red, "FILE FAILED AT BEING SAVED, CHECK CONSOLE FOR MORE INFO");
            else
                ImGui.TextColored(Vector4Colors.Green, "FILE SAVED SUCCESSFULLY");
        if (ImGui.Button("Save"))
            SaveData();
    }

    private void SaveData()
    {
        SerializedMaterialProperty[] properties = new SerializedMaterialProperty[m_uniforms.Length];
        for (int i = 0; i < properties.Length; i++)
            properties[i] = new(m_uniforms[i], m_types[i].Name, m_values[i]);
        SerializedMaterialData data = new(FileData.MaterialName, FileData.FragmentShaderRelativePath, FileData.VertexShaderRelativePath,
        m_isLit, properties);


        m_saveState = Cereal.SaveData<SerializedMaterialData>(data, FilePath);
        m_isDirty = false;
    }

    private void RenderProperty(int index)
    {
        try
        {
            var drawCall = m_typeToDrawMethod[m_types[index]];
            drawCall.Invoke(index, out bool wasChanged);
            if (!wasChanged)
                return;
            m_isDirty = true;
        }
        catch (Exception ex)
        {
            Scriptorium.LogException(ex);
        }
    }

    public override void Initialize(string filePath)
    {
        m_typeToDrawMethod.Add(typeof(float), DrawFloat);
        m_typeToDrawMethod.Add(typeof(AssetPath), DrawAssetPath);
        m_typeToDrawMethod.Add(typeof(Vector2), DrawVector2);
        m_typeToDrawMethod.Add(typeof(Vector3), DrawVector3);
        m_typeToDrawMethod.Add(typeof(Vector4), DrawVector4);
        m_typeToDrawMethod.Add(typeof(bool), DrawBool);

        FilePath = filePath;
        FileData = Cereal.LoadData<SerializedMaterialData>(filePath);
        m_uniforms = new string[FileData.Properties.Length];
        m_values = new object[FileData.Properties.Length];
        m_types = new Type[FileData.Properties.Length];
        for(int i = 0; i < FileData.Properties.Length; i++)
        {
            m_uniforms[i] = FileData.Properties[i].UniformName;
            m_values[i] = FileData.Properties[i].UniformValue;
            m_types[i] = Shader.GetTypeFor(FileData.Properties[i].UniformType);
        }
        m_isLit = FileData.IsLit;
    }

    private void DrawBool(int index, out bool wasChanged)
    {
        try
        {
            bool val = CastToIfNecessary<bool>(index);
            wasChanged = (ImGui.Checkbox(m_uniforms[index], ref val));
            if (wasChanged)
                m_values[index] = val;
        }
        catch (InvalidCastException ex)
        {
            Scriptorium.LogException(ex);
            wasChanged = false;
        }
    }

    private void DrawVector2(int index, out bool wasChanged)
    {
        try
        {
            Vector2 val = CastToIfNecessary<Vector2>(index);
            wasChanged = (ImGui.InputFloat2(m_uniforms[index], ref val));
            if (wasChanged)
                m_values[index] = val;
        }
        catch (InvalidCastException ex)
        {
            Scriptorium.LogException(ex);
            wasChanged = false;
        }
    }

    private void DrawVector3(int index, out bool wasChanged)
    {
        try
        {
            Vector3 val = CastToIfNecessary<Vector3>(index);
            wasChanged = (ImGui.InputFloat3(m_uniforms[index], ref val));
            if (wasChanged)
                m_values[index] = val;
        }
        catch (InvalidCastException ex)
        {
            Scriptorium.LogException(ex);
            wasChanged = false;
        }
    }

    private void DrawVector4(int index, out bool wasChanged)
    {
        try
        {
            Vector4 val = CastToIfNecessary<Vector4>(index);
            wasChanged = (ImGui.InputFloat4(m_uniforms[index], ref val));
            if (wasChanged)
                m_values[index] = val;
        }
        catch (InvalidCastException ex)
        {
            Scriptorium.LogException(ex);
            wasChanged = false;
        }
    }

    private void DrawAssetPath(int index, out bool wasChanged)
    {
        try
        {
            AssetPath aPath = CastToIfNecessary<AssetPath>(index);
            string path = aPath.AssetRelativePath;
            wasChanged = ImGuiFile.InputFile(m_uniforms[index], ref path);
            if (wasChanged)
                m_values[index] = new AssetPath(path);
        }
        catch (InvalidCastException ex)
        {
            Scriptorium.LogException(ex);
            wasChanged = false;
        }
    }

    private T? CastToIfNecessary<T>(int index)
    {
        T? castValue;
        if (m_values[index]!.GetType() == typeof(JObject))
        {
            castValue = ((JObject)m_values[index]!).ToObject<T>();
            m_values[index] = castValue;
        }
        else
            castValue = (T?)m_values[index];
        return castValue;
    }

    private void DrawFloat(int index, out bool wasChanged)
    {
        try
        {
            float val = (float)m_values[index];
            wasChanged = ImGui.InputFloat(CurrentSetProperty, ref val);
            if (wasChanged)
                m_values[index] = val;
        }
        catch(InvalidCastException ex)
        {
            Scriptorium.LogException(ex);
            wasChanged = false;
        }
    }
}