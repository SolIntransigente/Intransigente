using Dazed.Vitals;
using Dazed.Vitals.AssetReference;
using Dazed.Vitals.Localization;
using DazedGUITools.Windows.LocalizationTools;
using DazedGUITools.EditorCommons;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DazedGUITools.Windows.ExposedFieldManagement
{
    [MainMenuWindow("Exposed fields manager")]
    internal class ExposedFieldsManager : WindowBaseClass
    {
        private string _filter = string.Empty;

        private readonly List<ExposedField> _fields = [];
        private object?[] _values = [];

        private readonly FieldsDrawingHelper _drawingHelper;

        private ContextMessage? ExceptionMessageDisplay = null;
        private const string DELETE_FILE = "Disable file with all data (Will change the extension and thus will not be directly recognized by this program)";
        private const int DELETE_FILE_INDEX = 0;
        private const string IGNORE = "Ignore";
        private const int IGNORE_INDEX = 1;
        private static string[] ExceptionMessageOptions = [DELETE_FILE, IGNORE];

        private static string ExposedDataFilePath { get; } = Path.Combine(MainWindow.ProjectFolder, CommonPaths.ExposedDataFile);

        public ExposedFieldsManager() : base("Expose fields manager", true)
        {
            FindFields();
            _drawingHelper = new();
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
                if (!_fields[i].FieldFormalName.ToLower().Contains(_filter.ToLower()))
                    continue;
                if(_fields[i].ExpectedExtension != null)
                    ImGui.Text("Expects extension: " + _fields[i].ExpectedExtension);
                string label = $"From '{_fields[i].ClassName}' ({_fields[i].FieldType.Name})'{_fields[i].FieldFormalName}'";
                try
                {
                    if (_drawingHelper.DrawFor(label, _fields[i].FieldType, ref _values[i]))
                        IsDirty = true;
                }
                catch (Exception ex)
                {
                    if (ExceptionMessageDisplay == null)
                    {
                        StringBuilder sb = new();
                        sb.Append("There has been an error while displaying <");
                        sb.Append(_fields[i].FieldFormalName);
                        sb.Append("> field, with the following error message: \n");
                        sb.Append(ex.Message);
                        ExceptionMessageDisplay = new("Exception on exposed fields", sb.ToString(), ExceptionMessageOptions);
                    }
                    if (ExceptionMessageDisplay.Done)
                    {
                        InterpretMessageOption(ExceptionMessageDisplay.ResultValue);
                        ExceptionMessageDisplay = null;
                    }
                    break;
                }
                drawn = true;
            }
            if (!drawn)
                ImGui.Text("No item was found with the filter assigned.");
        }

        private void InterpretMessageOption(int resultValue)
        {
            if (resultValue == DELETE_FILE_INDEX)
                RenameDataFile();
        }

        private void RenameDataFile()
        {
            if (!Path.Exists(ExposedDataFilePath))
                return;
            string path = Path.Combine(MainWindow.ProjectFolder, CommonPaths.ExposedDataFileNoExtension + ".backup");
            using var f = File.Create(path);
            f.Dispose();
            File.Copy(ExposedDataFilePath, path, true);
            File.Delete(ExposedDataFilePath);
            FindFields();
        }

        // LocalizationItemSelection? _itemSelection;
        // private void LocalizedStringAssign(int i, string label)
        // {
        //     if (_values[i] == null)
        //         _values[i] = string.Empty;
        //     string str = (string)_values[i]!;
        //     if(ImGui.Button("Search##" + label) && _itemSelection == null)
        //         _itemSelection = new();
        //     if (_itemSelection != null)
        //         if (_itemSelection.Completed)
        //         {
        //             _values[i] = _itemSelection.ChosenItemIdentifier;
        //             IsDirty = true;
        //             _itemSelection = null;
        //         }
        //         else if (_itemSelection.MarkedForDisposal)
        //             _itemSelection = null;
        //     ImGui.SameLine();
        //     ImGui.InputText(label, ref str, (uint)short.MaxValue, ImGuiInputTextFlags.ReadOnly);
        // }

        // private void StringValAssign(int i, string label)
        // {
        //     if (_values[i] == null)
        //         _values[i] = string.Empty;
        //     string str = (string)_values[i]!;
        //     if (ImGui.InputText(label, ref str, (uint)short.MaxValue))
        //     {
        //         _values[i] = str;
        //         IsDirty = true;
        //     }
        // }

        // private void AssetPathAssign(int i, string label)
        // {
        //     if (_values[i] == null)
        //         _values[i] = string.Empty;
        //     string str = (string)_values[i]!;
        //     if (ImGuiFile.InputFile(label, ref str))
        //     {
        //         string nPath = Path.GetRelativePath(MainWindow.ProjectFolder, str);
        //         _values[i] = nPath[0] == '.' && nPath[1] == '.' ? "PATH IS NOT RELATIVE TO PROJECT FOLDER" : nPath;
        //         IsDirty = true;
        //     }
        // }

        private void Save()
        {
            string[] classAndField = new string[_values.Length];
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
                    _fields.Add(new(type.Name, att.FormalizedName, field.FieldType, att.ExpectedExtension));
                }
            }
            List<object?> values = [];
            foreach(ExposedField exF in _fields)
            {
                bool exists = ExposedData.TryRetrieve(exF.FormattedName, out var val, true);
                if (!exists)
                    values.Add(null);
                else
                    values.Add(val);
            }
            _values = values.ToArray();
        }
    }
}
