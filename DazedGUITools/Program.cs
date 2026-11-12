using System;

namespace DazedGUITools
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool forcePathSetting = args.Contains("-pathchange");
            Intransigente.AssemblyRelationCreate.CreateRelation();
            var window = new MainWindow(forcePathSetting);
            window.Run();
            ImguiImplOpenGL3.Shutdown();
            ImguiImplOpenTK4.Shutdown();
        }
    }
}
