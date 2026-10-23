using System.Diagnostics;

namespace Dazed.Vitals.VideoPlay
{
    internal class VideoAudio
    {
        private bool _disposed;
        private readonly Process _audioProcess;
        public VideoAudio(string videoPath)
        {
            _audioProcess = new();
            _audioProcess.StartInfo.CreateNoWindow = true;
            _audioProcess.StartInfo.FileName = PathUtils.BaseDirectoryCombine("ffmpeg\\ffplay.exe");
            _audioProcess.StartInfo.Arguments = $"{PathUtils.BaseDirectoryCombine(videoPath)} -nodisp -autoexit";
            Phone.SubscribeToApplicationQuit(Dispose);
        }

        ~VideoAudio()
        {
            Dispose();
        }

        internal void Start()
        {
            _audioProcess.Start();
        }

        internal void Dispose()
        {
            _disposed = true;
            _audioProcess.Kill();
            Phone.UnSubscribeToApplicationQuit(Dispose);
        }
    }
}
