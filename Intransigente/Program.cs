using Dazed;
using Dazed.Vitals;
using OpenTK.Windowing.Desktop;

namespace Intransigente
{
    internal class Program
    {
        private const int SecondsForProgramAnswer = 10;
        private static DazedInstance? _instance;
        private static ExceptionThrownMessage? _onException;
        private static bool _end;

        private static readonly List<string> _compiledLogs = new();
        static void Main(string[] args)
        {
            GameWindowSettings gSettings = new GameWindowSettings();
            NativeWindowSettings nSettings = new()
            {
                Title = "Intransigente",
                MinimumClientSize = new OpenTK.Mathematics.Vector2i(320, 240),
                AspectRatio = (640, 480),
                WindowState = OpenTK.Windowing.Common.WindowState.Maximized
            };
#if DEBUG
            ProgramFuncs.RetrieveDataFiles();
#endif
            ProgramFuncs.ProcessArguments(args);
            Scriptorium.OnEntryRecord += Log;
            _instance = new(gSettings, nSettings);
#if !DEBUG
            Task exitTask = new(LifeCheck);
            exitTask.Start();
            try
            {
#endif
                _instance.Run();
                _end = true;
#if !DEBUG
                exitTask.Wait();
                exitTask.Dispose();
            }
            catch(Exception ex)
            {
                SaveLogs();
#endif
                _end = true;
#if !DEBUG
                exitTask.Wait();
                exitTask.Dispose();
                _onException = new("EXCEPTION THROWN: " + ex.Message + " | WITH TRACE: " + ex.StackTrace);
                _onException.Run();
            }
#endif
            if (ProgramFuncs.StoreLogs && !_savedLogs)
                SaveLogs();
            Scriptorium.OnEntryRecord -= Log;
        }

        private static bool _savedLogs;
        private static void SaveLogs()
        {
            string content = "";
            foreach(string s in _compiledLogs)
            {
                content += s;
                content += "\n";
            }
            var now = DateTime.UtcNow;
            string path = Path.Combine(CommonPaths.LogsFolderName, $"{now.Second}-{now.Minute}-{now.Hour}-{now.Day}-{now.Month}-{now.Year}.log");
            path = PathUtils.BaseDirectoryCombine(path);
            if (!Directory.Exists(PathUtils.BaseDirectoryCombine(CommonPaths.LogsFolderName)))
                Directory.CreateDirectory(PathUtils.BaseDirectoryCombine(CommonPaths.LogsFolderName));
            using var f = File.Create(path);
            f.Dispose();
            File.WriteAllText(path, content);
            _savedLogs = true;
        }

        private static void LifeCheck()
        {
            if (DazedInstance.LastUpdateCall.ElapsedMilliseconds >= SecondsForProgramAnswer * 1000)
            {
                Scriptorium.Warn($"It's been {SecondsForProgramAnswer}s since last update from the game, and so it was terminated.");
                Environment.Exit(-1);
            }
            else
            {
                Thread.Sleep(100);
                if (_end)
                    return;
                LifeCheck();
            }
        }

        private static void Log(string message)
        {
            _compiledLogs.Add(message);
        }
    }
}
