namespace DazedGUITools.Windows.LevelEditor.LevelEditorTools
{
    public interface ILevelEditorTool
    {
        public string ToolName { get; }

        public void Initialize();
        public void Operate();
        public void Terminate();
    }
}
