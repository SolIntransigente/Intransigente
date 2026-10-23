using Dazed.Vitals.VideoPlay;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dazed.Vitals
{
    internal static class SplashScreen
    {
        private const string SplashScreenVideoName = "splashScreen.mp4";
        private static VideoPlayer _video;
        public static bool Ended { get; private set; }
        public static event Action? OnEndReached;

        public static void InitializeSplashScreen()
        {
            if (Ended) return;
            _video = new(SplashScreenVideoName);
            _video.VideoEndReached += EndSplashScreen;
        }

        private static void EndSplashScreen()
        {
            if (Ended) return;
            Ended = true;
            _video.Dispose();
            _video.VideoEndReached -= EndSplashScreen;
            OnEndReached?.Invoke();
        }

        internal static void Draw()
        {
        }
    }
}
