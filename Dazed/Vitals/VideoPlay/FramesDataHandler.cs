using Dazed.Vitals.Rendering;
using OpenTK.Graphics.OpenGL;

namespace Dazed.Vitals.VideoPlay;

internal class FramesDataHandler : IDisposable
{
    private byte[][] m_frames;
    private bool[] m_occupiedFrames;
    private uint[] m_framesAge;

    private int m_currentIndex = 0;
    private uint m_currentAge;

    private Dictionary<int, int> m_frameOrder;

    private readonly int m_width, m_height;

    public bool Vacancy 
    {
        get
        {
            return m_occupiedFrames.Contains(false); 
        }
    }
    private int m_handle;
    private int m_lastSetFrame = 0;
    public int FrameTextureHandle { get; private set; }
    public FramesDataHandler(int frameCapacity, int height, int width, int frameRate)
    {
        int bufferedFrames = frameRate * 10;
        m_frames = new byte[bufferedFrames][];
        m_occupiedFrames = new bool[bufferedFrames];
        m_framesAge = new uint[bufferedFrames];
        m_frameOrder = new(frameCapacity);
        m_width = width;
        m_height = height;
        m_handle = GL.GenTexture();
    }

    public void SetFrameData(int usedFrameIndex)
    {
        if(m_frameOrder.TryGetValue(usedFrameIndex, out int realIndex))
        {
            if(m_lastSetFrame == usedFrameIndex)
            {
                return;
            }
            for(int i = 0; i < m_frames.Length; i++)
                if(i == realIndex)
                    continue;
                else if(m_framesAge[i] < m_framesAge[realIndex])
                    m_occupiedFrames[i] = false;
            Scriptorium.Warn("Setting: " + usedFrameIndex + " at: " + realIndex);
            FrameTextureHandle = m_handle;
            GL.BindTexture(TextureTarget.Texture2D, FrameTextureHandle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R3G3B2, m_width, m_height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, m_frames[realIndex]);
            GL.TextureParameter(FrameTextureHandle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TextureParameter(FrameTextureHandle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            m_occupiedFrames[realIndex] = false;
            m_lastSetFrame = usedFrameIndex;
        }
        else if(DebugState.DebugLow)
            Scriptorium.Warn("Frame is not prepared, yet trying to be set for view");
    }

    public void AddFrame(in byte[] data)
    {
        for(int i = 0; i < m_frames.Length; i++)
        {
            if (m_occupiedFrames[i])
                continue;
            m_frameOrder.Add(m_currentIndex++, i);
            SetDataAtIndex(data, i);
            m_framesAge[i] = m_currentAge++;
            m_occupiedFrames[i] = true;
            return;
        }
        //Check the oldest frame and replace it.
        Scriptorium.Warn("All frames occupied");
        uint minAge = uint.MaxValue;
        int oldestFrame = -1;
        for(int i = 0; i < m_frames.Length; i++)
        {
            if (m_framesAge[i] >= minAge)
                continue;
            oldestFrame = i;
            minAge = m_framesAge[i];
        }
        ArgumentOutOfRangeException.ThrowIfEqual(oldestFrame, -1);
        m_framesAge[oldestFrame] = m_currentAge++;
        m_frameOrder.Add(m_currentIndex++, oldestFrame);
        SetDataAtIndex(data, oldestFrame);
    }

    private void SetDataAtIndex(byte[] data, int i)
    {
        if (m_frames[i] != null && m_frames[i].Length == data.Length)
            for (int b = 0; b < data.Length; b++)
                m_frames[i][b] = data[b];
        else
            m_frames[i] = data;
    }

    public void Dispose()
    {
        GL.DeleteTexture(FrameTextureHandle);
    }
}