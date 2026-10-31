using System.Runtime.InteropServices;

namespace Dazed.Vitals.VideoPlay
{
    internal unsafe class FrameData : IDisposable
    {
        private void* _memory;
        public int Length { get; private set; }
        private static readonly int _elementSize;
        static FrameData() => _elementSize = sizeof(byte);
        public FrameData(Array array)
        {
            Length = array.Length;
            _memory = (void*)Marshal.AllocHGlobal(Length * _elementSize);
            var data = (byte[])array;
            for(int i = 0; i < Length; i++)
                *this[i] = data[i];
        }
        public byte[] ToArray()
        {
            byte[] data = new byte[Length];
            for(int i = 0; i < Length; i++)
                data[i] = *this[i];
            return data;
        }
        public byte* this[int index]
        {
            get
            {
                return ((byte*)_memory) + _elementSize * index;
            }
        }
        public void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)_memory);
            _memory = null;
            Length = 0;
            GC.SuppressFinalize(this);
        }
    }
}
