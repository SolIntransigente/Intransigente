using Dazed.Vitals.Rendering;
using Emgu.CV;
using Emgu.CV.CvEnum;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace Dazed.Vitals.VideoPlay
{
    internal class VideoPlayer : Renderable
    {
        private bool _disposed;
        private readonly string _path;
        private readonly VideoCapture _capture;
        private readonly VideoAudio _audio;
        private int _currentOffset;
        // private readonly FrameData[] _framesData;
        private readonly FramesDataHandler m_handler;
        private readonly float _interval, _aspectRatio;
        private readonly uint _frameCount, m_framesPerSecond;
        private int _currentFrame;
        private readonly short _width, _height;

        private short _dWidth, _dHeight;
        private short _dX, _dY;

        private readonly Stopwatch _elapsedSinceUpdate;

        public override bool AllowRendering => IsVisible;
        public override RenderingPass RenderPass => RenderingPass.userInterface;
        public bool IsVisible { get; set; } = true;

        public event Action? VideoEndReached;

        private readonly FullscreenRenderer _videoPlayerRenderer;
        public VideoPlayer(string path)
        {
            _capture = new VideoCapture(path);
            _audio = new VideoAudio(path);
            //_audioPlayer = new(Options);
            if (!_capture.IsOpened)
                throw new Exception("An error was found while assigning the video capture");
            _path = path;
            _videoPlayerRenderer = new(QuadData.QuadShader);
            _elapsedSinceUpdate = new();

            var fps = (byte)_capture.Get(CapProp.Fps);
            _interval = 1000f / fps;
            m_framesPerSecond = fps;
            _frameCount = (uint)_capture.Get(CapProp.FrameCount);
            _width = (short)_capture.Get(CapProp.FrameWidth);
            _height = (short)_capture.Get(CapProp.FrameHeight);
            _aspectRatio = (float)_width / _height;
            // _textureHandle = GL.GenTexture();
            // _framesData = new FrameData[_frameCount];
            m_handler = new((int)_frameCount, _height, _width, (int) m_framesPerSecond);

            _capture.ImageGrabbed += ImageGrabbed;
            Artist.OnWidowResize += WidthHeightSetUp;
            Phone.SubscribeToApplicationQuit(OnQuit);
            _capture.Start();
            _audio.Start();
            WidthHeightSetUp();
        }
        private void OnQuit()
        {
            _audio.Dispose();
            Phone.UnSubscribeToApplicationQuit(OnQuit);
        }
        private int m_latestSentIndex = 0;
        private void ImageGrabbed(object? sender, EventArgs e)
        {
            var mat = new Mat();
            if (!_capture.Retrieve(mat))
                throw new Exception("An error occurred while grabbing a new frame");
            var thisIndex = m_latestSentIndex++;
            // while (!m_handler.Vacancy)
            // {
            //     Scriptorium.Warn("Waiting | Ahead");
            //     Thread.Sleep(10);
            // }
            m_handler.AddFrame((byte[])mat.GetData(false));
            // _framesData[_currentOffset++] = new(mat.GetData(false));
        }

        public new void Dispose()
        {
            if (_disposed)
                return;
            base.Dispose();
            _capture.Stop();
            _capture.Dispose();
            _disposed = true;
            Artist.OnWidowResize -= WidthHeightSetUp;
            m_handler.Dispose();
            // GL.DeleteTexture(_textureHandle);
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
            if (_currentFrame + 1 >= _frameCount)
                return;
            if (!_elapsedSinceUpdate.IsRunning)
                _elapsedSinceUpdate.Start();

            _currentFrame = (int)(_elapsedSinceUpdate.ElapsedMilliseconds / _interval);
            // if (_currentFrame >= _framesData.Length)
            // {
            //     int count = 0;
            //     while (true)
            //     {
            //         if (_currentFrame < _framesData.Length)
            //             break;
            //         count++;
            //         if (count >= short.MaxValue)
            //             return;
            //     }
            // }
            // PrepareFrame(ref _framesData[_currentFrame]);
            GL.Viewport(_dX, _dY, _dWidth, _dHeight);
            m_handler.SetFrameData(_currentFrame);
            _videoPlayerRenderer.Render(m_handler.FrameTextureHandle);
            GL.Viewport(0, 0, Artist.Width, Artist.Height);
        }

        // private void PrepareFrame(ref FrameData frame)
        // {
        //     GL.BindTexture(TextureTarget.Texture2D, _textureHandle);
        //     try
        //     {
        //         if (frame != null)
        //         {
        //             byte[] pixels = frame.ToArray();
        //             GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R3G3B2, _width, _height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, pixels);
        //             GL.TextureParameter(_textureHandle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        //             GL.TextureParameter(_textureHandle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        //         }
        //     }
        //     catch(Exception ex)
        //     {
        //         Scriptorium.Error($"Frame preparation for frame at index <{_currentFrame}> for file <{_path}> failed with the following exception");
        //         Scriptorium.LogException(ex);
        //     }
        //     finally
        //     {
        //         frame?.Dispose();
        //         GL.BindTexture(TextureTarget.Texture2D, 0);
        //     }
        // }
    }
}
