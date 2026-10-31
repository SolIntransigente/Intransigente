using System;
using System.Collections.Generic;
using System.Text;

namespace Dazed.Vitals
{
    public static class Chronos
    {
        public static float UnscaledDeltaTime { get => _deltaTime; }
        public static float DeltaTime { get => _deltaTime * _timeScale; }
        private static float _deltaTime;
        public static float TimeScale { get => _timeScale; set => _timeScale = value; }
        private static float _timeScale = 1;

        internal static void UpdateDeltaTime(float deltaTime)
        {
            _deltaTime = deltaTime;
        }

        public static void ExternalUpdate(float deltaTime)
        {
            _deltaTime = deltaTime;
        }
    }
}
