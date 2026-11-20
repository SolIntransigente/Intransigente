using Dazed.Vitals.VideoPlay;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dazed.Vitals
{
    internal class SplashScreen
    {
        private const string SplashScreenVideoName = "splashScreen.mp4";
        private readonly VideoPlayer m_video;
        public bool Disposed { get; private set; }
        public event Action? OnEndReached;

        public SplashScreen()
        {
            m_video = new(PathUtils.BaseDirectoryCombine(SplashScreenVideoName));
            m_video.VideoEndReached += EndSplashScreen;
        }

        public void Start()
        {
            m_video.Start();
        }

        private void EndSplashScreen()
        {
            if (Disposed) return;
            Disposed = true;
            m_video.VideoEndReached -= EndSplashScreen;
            OnEndReached?.Invoke();
        }
    }
}
