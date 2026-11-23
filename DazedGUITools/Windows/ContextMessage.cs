using ImGuiNET;

namespace DazedGUITools.Windows;

public sealed class ContextMessage : WindowBaseClass
{
    public bool Done => this.MarkedForDisposal;
    public int ResultValue { get; private set; } = -1;
    private readonly string m_message;
    private readonly string[] m_options;
    private readonly uint thisIndex;
    private static uint CurrentIndex;
    public ContextMessage(string title, string message, string[] options) : base(title, false)
    {
        m_options = options;
        m_message = message;
        thisIndex = CurrentIndex++;
    }

    protected override void Draw()
    {
        ImGui.Text(m_message);
        for (int i = 0; i < m_options.Length; i++)
            if (ImGui.Button(m_options[i] + "##" + thisIndex))
                ResultValue = i;
        if(ResultValue != -1)
            Close();
    }
}