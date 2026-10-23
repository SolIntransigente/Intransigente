using System;
using System.Collections.Generic;
using System.Text;

namespace Dazed.Vitals.VideoPlay
{
    internal readonly struct FrameData
    {
        
        static FrameData()
        {
            empty = new(Array.Empty<byte>());
        }
        public readonly byte[] Data;
        private static readonly FrameData empty;
        public static FrameData Empty => empty;
        public FrameData(byte[] data)
        {
            Data = data;
        }
    }
}
