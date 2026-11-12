using Dazed.Vitals;
using Dazed.Vitals.AssetReference;
using Dazed.Vitals.Localization;
using DazedGUITools.Windows.LocalizationTools;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DazedGUITools.Windows.ExposedFieldManagement
{
    [MainMenuWindow("Exposed fields manager")]
    internal class ExposedFieldsManager : WindowBaseClass
    {
        private string _filter = string.Empty;

        private readonly List<ExposedField> _fields = [];
        private readonly List<object?> _values = [];

        public ExposedFieldsManager() : base("Expose fields manager", true)
        {
            FindFields();
        }

        protected override void Draw()
        {
            if (IsDirty)
                ImGui.TextColored(Vector4Colors.Red, "THERE ARE UNSAVED CHANGES");
            if (ImGui.Button("Close"))
                Close();
            if (ImGui.Button("Save"))
                Save();
            ImGui.Text("Finds fields marked to be exposed and allows assigning a value to them.");
            ImGui.Separator();

            ImGui.InputText("Filter", ref _filter, (uint)short.MaxValue);
            ImGui.Text("Filter is not case sensitive");
            ImGui.Separator();
            bool drawn = false;
            for (int i = 0; i < _fields.Count; i++)
            {
                if (!_fields[i].FieldFormalName.ToLower().Contains(_filter))
                    continue;
                string label = $"({_fields[i].FieldType.Name})'{_fields[i].FieldFormalName}' from class '{_fields[i].ClassName}'";
                if (_fields[i].FieldType == typeof(string))
                {
                    StringValAssign(i, label);
                }
                else if (_fields[i].FieldType == typeof(AssetPath))
                {
                    AssetPathAssign(i, label);
                }
                else if (_fields[i].FieldType == typeof(LocalizedString))
                {
                    LocalizedStringAssign(i, label);
                }
                drawn = true;
            }
            if (!drawn)
                ImGui.Text("No item was found with the filter assigned.");
        }

        LocalizationItemSelection? _itemSelection;
        private void LocalizedStringAssign(int i, string label)
        {
            if (_values[i] == null)
                _values[i] = string.Empty;
            string str = (string)_values[i]!;
            if(ImGui.Button("Search##" + label) && _itemSelection == null)
                _itemSelection = new();
            if (_itemSelection != null)
                if (_itemSelection.Completed)
                {
                    _values[i] = _itemSelection.ChosenItemIdentifier;
                    IsDirty = true;
                    _itemSelection = null;
                }
                else if (_itemSelection.MarkedForDisposal)
                    _itemSelection = null;
            ImGui.SameLine();
            ImGui.InputText(label, ref str, (uint)short.MaxValue, ImGuiInputTextFlags.ReadOnly);
        }

        private void StringValAssign(int i, string label)
        {
            if (_values[i] == null)
                _values[i] = string.Empty;
            string str = (string)_values[i]!;
            if (ImGui.InputText(label, ref str, (uint)short.MaxValue))
            {
                _values[i] = str;
                IsDirty = true;
            }
        }

        private void AssetPathAssign(int i, string label)
        {
            if (_values[i] == null)
                _values[i] = string.Empty;
            string str = (string)_values[i]!;
            if (ImGuiFile.InputFile(label, ref str))
            {
                string nPath = Path.GetRelativePath(MainWindow.ProjectFolder, str);
                _values[i] = nPath[0] == '.' && nPath[1] == '.' ? "PATH IS NOT RELATIVE TO PROJECT FOLDER" : nPath;
                IsDirty = true;
            }
        }

        private void Save()
        {
            string[] classAndField = new string[_values.Count];
            for (int i = 0; i < classAndField.Length; i++)
                classAndField[i] = _fields[i].FormattedName;
            ExposedDataArrays data = new(classAndField, _values.ToArray());
            Cereal.SaveData(data, MainWindow.ProjectFolder, CommonPaths.ExposedDataFile);
            IsDirty = false;
        }

        private void FindFields()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies().ToArray();
            Type[] types = assemblies.SelectMany(x => x.GetExportedTypes()).ToArray();
            foreach (var type in types)
            {
                var fields = type.GetFields(ExposedData.ExposedFieldFlags);
                foreach(var field in fields)
                {
                    if (!field.GetCustomAttributes(true).Any(x => x.GetType() == typeof(ExposedFieldAttribute)))
                        continue;
                    ExposedFieldAttribute att = (ExposedFieldAttribute)field.GetCustomAttributes(true).First(x => x.GetType() == typeof(ExposedFieldAttribute));
                    _fields.Add(new(type.Name, att.FormalizedName, field.FieldType));
                }
            }
            foreach(ExposedField exF in _fields)
            {
                bool exists = ExposedData.TryRetrieve(exF.FormattedName, out var val);
                if (!exists)
                    _values.Add(null);
                else
                    _values.Add(val);
            }
        }
    }
}
