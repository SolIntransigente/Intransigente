using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Dazed.Vitals;
using Dazed.Vitals.AssetReference;
using Dazed.Vitals.Localization;
using DazedGUITools.Windows;
using DazedGUITools.Windows.LocalizationTools;
using ImGuiNET;

namespace DazedGUITools.EditorCommons;

public class FieldsDrawingHelper
{
    private delegate bool DrawingFunction(string name, ref object? value);
    private readonly Dictionary<Type, DrawingFunction> AllFuncs = [];

    public FieldsDrawingHelper()
    {
        AllFuncs.Add(typeof(LocalizedString), LocalizedStringAssign);
        AllFuncs.Add(typeof(AssetPath), AssetPathAssign);
        AllFuncs.Add(typeof(string), StringAssign);
        AllFuncs.Add(typeof(Vector3), Vec3Assign);
        AllFuncs.Add(typeof(Vector2), Vec2Assign);
        AllFuncs.Add(typeof(float), FloatAssign);
        AllFuncs.Add(typeof(int), IntAssign);
        AllFuncs.Add(typeof(uint), UintAssign);
        AllFuncs.Add(typeof(bool), BooleanAssign);
    }

    public bool DrawFor(string label, Type valType, ref object? value)
    {
        Type typeOfVal = valType;
        if (!AllFuncs.ContainsKey(typeOfVal))
        {
            Scriptorium.Error($"There is no function for drawing fields of type: {typeOfVal.Name}");
            return false;
        }
        var func = AllFuncs[typeOfVal];
        var result = func.Invoke(label, ref value);
        return result;
    }

    LocalizationItemSelection? _itemSelection;
    private bool LocalizedStringAssign(string label, ref object? value)
    {
        if (value == null)
            value = string.Empty;
        string str = (string)value;
        if (ImGui.Button("Search##" + label) && _itemSelection == null)
            _itemSelection = new();
        var state = false;
        if (_itemSelection != null)
            if (_itemSelection.Completed)
            {
                value = _itemSelection.ChosenItemIdentifier!;
                _itemSelection = null;
                state = true;
            }
            else if (_itemSelection.MarkedForDisposal)
                _itemSelection = null;
            ImGui.SameLine();
            ImGui.InputText(label, ref str, (uint)short.MaxValue, ImGuiInputTextFlags.ReadOnly);
            return state;
        }

        private bool StringAssign(string label, ref object? value)
        {
            if (value == null)
                value = string.Empty;
            string str = (string)value!;
            if (ImGui.InputText(label, ref str, (uint)short.MaxValue))
            {
                value = str;
                return true;
            }
            return false;
        }

        private bool AssetPathAssign(string label, ref object? value)
        {
            if (value == null)
                value = AssetPath.NoPathAssigned;
            string str = (string)value!;
            if (ImGuiFile.InputFile(label, ref str))
            {
                string nPath = Path.GetRelativePath(MainWindow.ProjectFolder, str);
                //If two first characters are a dot, then it is a folder outside of the root
                value = nPath[0] == '.' && nPath[1] == '.' ? "PATH IS NOT RELATIVE TO PROJECT FOLDER" : nPath;
                return true;
            }
            return false;
        }

        private bool Vec3Assign(string label, ref object? value)
        {
            if (value == null)
                value = Vector3.Zero;
            Vector3 val = (Vector3)value;
            if (ImGui.InputFloat3(label, ref val))
            {
                value = val;
                return true;
            }
            return false;
        }
        private bool Vec2Assign(string label, ref object? value)
        {
            if (value == null)
                value = Vector2.Zero;
            Vector2 val = (Vector2)value;
            if (ImGui.InputFloat2(label, ref val))
            {
                value = val;
                return true;
            }
            return false;
        }
        private bool FloatAssign(string label, ref object? value)
        {
            if (value == null)
                value = 0f;
            float val = (float)value;
            if (ImGui.InputFloat(label, ref val))
            {
                value = val;
                return true;
            }
            return false;
        }
        private bool IntAssign(string label, ref object? value)
        {
            if (value == null)
                value = (int)0;
            int val = (int)value;
            if (ImGui.InputInt(label, ref val))
            {
                value = val;
                return true;
            }
            return false;
        }
        private bool UintAssign(string label, ref object? value)
        {
            if (value == null)
                value = (uint)0;
            int val = (int)MathTeacher.GetSafeUnsignedInteger((int)value);
            if (ImGui.InputInt(label, ref val))
            {
                value = MathTeacher.GetSafeUnsignedInteger(val);
                return true;
            }
            return false;
        }
        private bool BooleanAssign(string label, ref object? value)
        {
            if (value == null)
                value = false;
            bool val = (bool)value;
            if (ImGui.Checkbox(label, ref val))
            {
                value = val;
                return true;
            }
            return false;
        }
}