using SoundFlow.Components;
namespace Dazed.Vitals.Audio
{
    public class SoundEmitter : IDisposable
    {
        public SoundPlayer? Player => _playerExists && !_isDisposed ? _player : null;
        public AudioClip Clip => _clip;
        private SoundPlayer _player = null!;
        private AudioClip _clip = null!;
        private bool _playerExists, _isDisposed;

        public float Volume
        { get => Settings.Volume; set => _settings.Volume = value; }
        public float Panning 
        { get => Settings.Panning; set => _settings.Panning = value; }
        public float PlaySpeed 
        { get => Settings.Speed; set => _settings.Speed = value; }
        public bool Loop 
        { get => Settings.Loop; set => _settings.Loop = value; }
        public bool PlayOnStart 
        { get => Settings.PlayOnStart; set => _settings.PlayOnStart = value; }
        public bool ThreeDimensionalAudio 
        { get => Settings.ThreeDimensionalAudio; set => _settings.ThreeDimensionalAudio = value; }

        private SoundEmissionSettings _settings;
        private SoundEmissionSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                SetPlayerSettings();
            }
        }

        public SoundEmitter(string pathToClip, SoundEmissionSettings? settings = null)
        {
            settings ??= SoundEmissionSettings.Default;
            Settings = settings.Value;
            _clip = new AudioClip(pathToClip);
            SetNewPlayer(_clip);
            SetPlayerSettings();
            OmniSonus.OnTermination += Dispose;
        }

        public void ChangeClip(string clipPath)
        {
            _player?.Stop();
            _player?.Dispose();
            _clip = new(clipPath);
            SetNewPlayer(_clip);
            SetPlayerSettings();
        }

        private void SetPlayerSettings()
        {
            if (!_playerExists)
                return;
            _player.IsLooping = Settings.Loop;
            _player.Volume = Settings.Volume;
            _player.Pan = Settings.Panning;
            _player.PlaybackSpeed = Settings.Speed;
            if (Settings.PlayOnStart)
                _player.Play();
        }

        private void SetNewPlayer(AudioClip clip)
        {
            if (_player != null)
                OmniSonus.MainDevice.MasterMixer.RemoveComponent(_player);
            _player = new(OmniSonus.Engine, OmniSonus.Format, clip.Provider);
            OmniSonus.MainDevice.MasterMixer.AddComponent(_player);
            _playerExists = true;
        }

        public void Play()
        {
            if (!_playerExists) return;
            _player.Play();
        }
        public void Pause()
        {
            if (!_playerExists) return;
            _player.Pause();
        }
        public void Stop()
        {
            if (!_playerExists) return;
            _player.Stop();
        }
        public void Restart()
        {
            if (!_playerExists) return;
            _player.Stop();
            _player.Play();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            _player.Stop();
            OmniSonus.MainDevice.MasterMixer.RemoveComponent(_player);
            _playerExists = false;
            _isDisposed = true;
            OmniSonus.OnTermination -= Dispose;
        }
    }
}
