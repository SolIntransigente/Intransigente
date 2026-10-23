using SoundFlow.Providers;

namespace Dazed.Vitals.Audio
{
    public class AudioClip
    {
        public readonly string Path;
        public StreamDataProvider Provider => _provider;
        private readonly StreamDataProvider _provider;

        public static void CreateAudioClip(string path, out AudioClip clip)
        {
            clip = new AudioClip(path);
        }

        internal AudioClip(string path)
        {
            Path = path;
            if (!File.Exists(Path))
                throw new NullReferenceException($"File at path |{Path}| does not exist");
            var fileStream = File.OpenRead(Path);
            _provider = new StreamDataProvider(OmniSonus.Engine, OmniSonus.Format, fileStream);
        }
    }
}
