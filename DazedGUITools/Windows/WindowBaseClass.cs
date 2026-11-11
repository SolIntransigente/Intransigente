using Dazed.Vitals;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace DazedGUITools.Windows
{
    public abstract class WindowBaseClass
    {
        internal readonly string _uniqueTitle;
        public readonly string Title;
        public readonly Guid UniqueWindowID;
        /// <summary>
        /// Meaning, there can be only one instance of it at a time
        /// </summary>
        public readonly bool ExclusiveWindow;

        public bool MarkedForDisposal { get; private set; }
        public bool IsDirty { get;  protected set; }

        public WindowBaseClass(string title, bool exclusive)
        {
            Title = title;
            UniqueWindowID = Guid.NewGuid();
            ExclusiveWindow = exclusive;
            _uniqueTitle = GetUniqueName(title, out bool successful);
            if (!successful)
            {
                Scriptorium.Warn($"There can only be one window of type [{GetType()}] at a time.");
                return;
            }
            MainWindow.AddWindow(this);
            OnInitialize();
        }

        protected virtual void OnInitialize() { }

        private string GetUniqueName(string title, out bool successful, int triesCount = 0)
        {
            successful = true;
            string currentTitle = title + (triesCount > 0 ? ": " + triesCount.ToString() : "");
            if (!MainWindow.NameIsOccupied(currentTitle))
                return currentTitle;
            if (ExclusiveWindow)
            {
                successful = false;
                return "";
            }
            return GetUniqueName(title, out successful, ++triesCount);
        }
        /// <summary>
        /// Marks window for disposal after completing the current draw cycle.
        /// </summary>
        internal void Close()
        {
            MarkedForDisposal = true;
            OnClosed();
        }

        internal virtual void OnClosed() { }

        internal void CallDraw()
        {
            if (MarkedForDisposal)
                return;
            if (ImGui.Begin(_uniqueTitle))
                Draw(); 
            ImGui.End();
        }
        protected abstract void Draw();

        protected bool InputVector3(string text, ref Vector3 value)
        {
            var vec3 = value;
            ImGui.Text(text);
            if (ImGui.InputFloat($"##{text}x", ref vec3.X))
            {
                value.X = vec3.X;
                return true;
            }
            if(ImGui.InputFloat($"##{text}y", ref vec3.Y))
            {
                value.Y = vec3.Y;
                return true;
            }
            if(ImGui.InputFloat($"##{text}z", ref vec3.Z))
            {
                value.Z = vec3.Z;
                return true;
            }
            return false;
        }
    }
}
