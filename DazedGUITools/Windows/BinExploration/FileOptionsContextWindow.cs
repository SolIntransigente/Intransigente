using Dazed.Vitals;
using Dazed.Vitals.Content;
using DazedGUITools.Windows.BinExploration.FileInfoDrawing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DazedGUITools.Windows.BinExploration
{
    internal class FileOptionsContextWindow : WindowBaseClass
    {
        readonly string _currentRelativePath, _fileName;
        string RelativeFilePath => Path.Combine(_currentRelativePath, _fileName);

        private readonly FileInfoDrawingBase? DrawingFunction;

        public FileOptionsContextWindow(string fileName, string currentRelativePath) : base("File Context", true)
        {
            _currentRelativePath = currentRelativePath;
            _fileName = fileName;
            DrawingFunction = GetProperDrawingFunction();
            try
            {
                if (DrawingFunction != null)
                    DrawingFunction.Initialize(Path.Combine(MainWindow.ProjectFolder, Path.Combine(_currentRelativePath, _fileName)));
            }
            catch(Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }

        private FileInfoDrawingBase? GetProperDrawingFunction()
        {
            //TODO: Create dictionary of functions based on file extension, and populate it with the necessary values
            //TODO: Change the material extension with a static reference to CommonPaths
            if(_fileName.GetExtension().ToLower() == ".material")
                return new MaterialInfoDrawing();
            return null;
        }

        protected override void Draw()
        {
            ImGui.Text($"For file: {RelativeFilePath}");
            if (DrawingFunction != null)
                DrawingFunction.Draw();
            if (ImGui.Button("Close"))
                Close();
        }
    }
}
