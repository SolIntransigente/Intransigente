using Dazed.Vitals;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace DazedGUITools.Windows
{
    internal class StartUpWindow : FileManagedWindow
    {
        public string ResultPath { get; private set; } = "";
        string _currentSetPath = "";

        private bool _foundConfig;

        public StartUpWindow() : base("Starting up...", true)
        {
            if (File.Exists(PathUtils.BaseDirectoryCombine("path.cfg")))
            {
                var content = File.ReadAllText(PathUtils.BaseDirectoryCombine("path.cfg"));
                if(content == "")
                    return;
                var settings = JsonConvert.DeserializeObject<string>(content);
                ResultPath = settings;
                _foundConfig = true;
            }
        }

        protected override void Draw()
        {
            if (_foundConfig)
            {
                if(ImGui.Button("Previously assigned folder"))
                {
                    Close();
                }
                if(ImGui.Button("Another folder"))
                {
                    _foundConfig = false;
                }
                return;
            }
            ImGui.Text("Please select a path to use for resources and shaders");
            PathValidMessage(_currentSetPath, out bool pathIsCompatible);
            InputDirectory("Path", ref _currentSetPath);
            if(ImGui.Button("Select") && pathIsCompatible)
            {
                ResultPath = _currentSetPath;
                SavePath();
                Close();
            }
        }

        internal override void OnClosed()
        {
            PathUtils.OverwrittenBasePath = ResultPath;
            PathUtils.IsBasePathOverwritten = true;
        }

        private void SavePath()
        {
            var content = JsonConvert.SerializeObject(ResultPath);
            var path = PathUtils.BaseDirectoryCombine("path.cfg");
            using var f = File.Create(path);
            f.Dispose();
            File.WriteAllText(path, content);
        }
    }
}
