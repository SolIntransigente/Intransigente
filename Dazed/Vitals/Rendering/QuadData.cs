using System;
using System.Collections.Generic;
using System.Text;

namespace Dazed.Vitals.Rendering
{
    public static class QuadData
    {
        public static readonly Shader QuadShader;

        static QuadData()
        {
            QuadShader = Shader.GetShaderInstance("quadShader.vert", "screenShader.frag");
        }
        public static readonly float[] VertexPosAndTexturePosData =
        {
            // Position     Texture coordinates
             1f,  1f, 0.0f, 1.0f, 0.0f, // top right
             1f, -1f, 0.0f, 1.0f, 1.0f, // bottom right
            -1f, -1f, 0.0f, 0.0f, 1.0f, // bottom left
            -1f,  1f, 0.0f, 0.0f, 0.0f  // top left
        };
        /// <summary>
        /// Inversed texture coords version
        /// </summary>
        public static readonly float[] VertexPosAndInvTexturePosData =
        {
            // Position     Texture coordinates
             1f,  1f, 0.0f, 1.0f, 0.0f, // top right
             1f, -1f, 0.0f, 1.0f, 1.0f, // bottom right
            -1f, -1f, 0.0f, 0.0f, 1.0f, // bottom left
            -1f,  1f, 0.0f, 0.0f, 0.0f  // top left
        };

        public static readonly float[] VertexPosData =
        {
            1f,  1f, 0.0f,
            1f, -1f, 0.0f,
            -1f, -1f, 0.0f,
            -1f,  1f, 0.0f
        };

        public static readonly uint[] Indices =
        {
            0, 1, 3,
            1, 2, 3
        };
    }
}
