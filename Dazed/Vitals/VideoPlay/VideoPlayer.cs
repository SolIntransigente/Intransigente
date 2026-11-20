using Dazed.Vitals.Rendering;
using Emgu.CV;
using Emgu.CV.CvEnum;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace Dazed.Vitals.VideoPlay
{
    internal class VideoPlayer : Renderable
    {
        private bool m_disposed;
        private readonly string m_path;
        private VideoCapture m_capture = null!;
        private VideoAudio m_audio = null!;
        private FramesDataHandler m_handler;
        private float _interval, _aspectRatio;
        private uint _frameCount, m_framesPerSecond;
        private int _currentFrame;
        private short _width, _height;

        private short _dWidth, _dHeight;
        private short _dX, _dY;

        private Stopwatch _elapsedSinceUpdate;

        public readonly bool Loops;
        private bool m_started;
        public override bool AllowRendering => IsVisible;
        public override RenderingPass RenderPass => RenderingPass.userInterface;
        public bool IsVisible { get; set; } = true;

        public event Action? VideoEndReached;

        private FullscreenRenderer _videoPlayerRenderer;
        public VideoPlayer(string path, bool autoStart = false, bool loops = false)
        {
            m_path = path;
            Loops = loops;
            if (autoStart)
                Start();
        }

        public void Start()
        {
            if (m_started)
                return;
            m_started = true;
            m_capture = new VideoCapture(m_path);
            m_audio = new VideoAudio(m_path);
            //_audioPlayer = new(Options);
            if (!m_capture.IsOpened)
                throw new Exception("An error was found while assigning the video capture");
            _videoPlayerRenderer = new(QuadData.QuadShader);
            _elapsedSinceUpdate = new();

            var fps = (byte)m_capture.Get(CapProp.Fps);
            _interval = 1000f / fps;
            m_framesPerSecond = fps;
            _frameCount = (uint)m_capture.Get(CapProp.FrameCount);
            _width = (short)m_capture.Get(CapProp.FrameWidth);
            _height = (short)m_capture.Get(CapProp.FrameHeight);
            _aspectRatio = (float)_width / _height;
            m_handler = new((int)_frameCount, _height, _width, (int)m_framesPerSecond);

            m_capture.ImageGrabbed += ImageGrabbed;
            Artist.OnWidowResize += WidthHeightSetUp;
            Phone.SubscribeToApplicationQuit(OnQuit);
            m_capture.Start();
            m_audio.Start();
            WidthHeightSetUp();
        }

        private void OnQuit()
        {
            m_audio.Dispose();
            Phone.UnSubscribeToApplicationQuit(OnQuit);
        }
        private void ImageGrabbed(object? sender, EventArgs e)
        {
            var mat = new Mat();
            if (!m_capture.Retrieve(mat))
                throw new Exception("An error occurred while grabbing a new frame");
            m_handler.AddFrame((byte[])mat.GetData(false));
        }

        public new void Dispose()
        {
            if (m_disposed)
                return;
            m_disposed = true;
            base.Dispose();
            m_capture.Stop();
            m_capture.Dispose();
            Artist.OnWidowResize -= WidthHeightSetUp;
            m_handler.Dispose();
        }

        public void WidthHeightSetUp()
        {
            _dHeight = (short)Artist.Height;
            _dWidth = (short) Artist.Width;
            if (Artist.Width * _width > Artist.Height * _height)
            {
                _dWidth = (short)(Artist.Height * _aspectRatio);
                _dX = (short)((Artist.Width - _dWidth) / 2f);
                _dY = 0;
            }
            else if (Artist.Width * _width < Artist.Height * _height)
            {
                _dHeight = (short)(Artist.Width * _aspectRatio);
                _dY = (short)((Artist.Height - _dHeight) / 2f);
                _dX = 0;
            }
        }

        public override void Render(in RenderingData data)
        {
            if (!m_started)
                return;
            if (_currentFrame + 1 >= _frameCount)
            {
                if (Loops)
                {
                    RestartFrameCatch();
                    return;
                }
                if (m_disposed)
                    return;
                Dispose();
                VideoEndReached?.Invoke();
                return;
            }
            if (!_elapsedSinceUpdate.IsRunning)
                _elapsedSinceUpdate.Start();

            _currentFrame = (int)(_elapsedSinceUpdate.ElapsedMilliseconds / _interval);
            GL.Viewport(_dX, _dY, _dWidth, _dHeight);
            m_handler.SetFrameData(_currentFrame);
            _videoPlayerRenderer.Render(m_handler.FrameTextureHandle);
            GL.Viewport(0, 0, Artist.Width, Artist.Height);
        }

        private void RestartFrameCatch()
        {
            throw new NotImplementedException();
        }
    }
}
