using System;
using System.Numerics;
using Dazed.Vitals;

namespace DazedGUITools.Windows.LevelEditor.Representations
{
    public class VertexData
    {
        ///<summary>
        ///Specifies whether the vertex does NOT depend on other vertex 
        ///</summary>
        public readonly bool IsAbsoluteVertex;
        public Vector3 Position
        {
            get
            {
                if (ParentedVertices == null)
                    return m_pos;
                else
                {
                    Vector3 value = new();
                    if (Flag.useX0)
                        value.X = v0.Position.X;
                    else
                        value.X = v1.Position.X;
                    if (Flag.useY0)
                        value.Y = v0.Position.Y;
                    else
                        value.Y = v1.Position.Y;
                    if (Flag.useZ0)
                        value.Z = v0.Position.Z;
                    else
                        value.Z = v1.Position.Z;
                    return value;
                }
            }
            set
            {
                if(ParentedVertices == null)
                {
                    m_pos = value;
                }
                else
                {
                    if(Flag.useX0 || Flag.useY0 || Flag.useZ0)
                    {
                        Vector3 pos0 = new Vector3(Flag.useX0 ? value.X : v0.Position.X, Flag.useY0 ? value.Y : v0.Position.Y, Flag.useZ0 ? value.Z : v0.Position.Z);
                        ParentedVertices[0].Position = pos0;
                    }
                    if(!Flag.useX0 || !Flag.useY0 || !Flag.useZ0)
                    {
                        Vector3 pos1 = new Vector3(!Flag.useX0 ? value.X : v1.Position.X, !Flag.useY0 ? value.Y : v1.Position.Y, !Flag.useZ0 ? value.Z : v1.Position.Z);
                        ParentedVertices[1].Position = pos1;
                    }
                }
            }
        }
        public Vector3 Normal
        {
            get => ParentedVertices == null ? m_normal : v0.Normal;
            internal set
            {
                if (ParentedVertices != null)
                    return;
                m_normal = value;
            }
        }

        internal bool m_useZtoY, m_useZtoX;

        private Vector2 m_overwrittenTexCoord;
        private bool m_overwriteTexCoord;

        internal bool m_flipX, m_flipY;

        private Vector3 m_normal;
        private Vector3 m_pos;
        public Vector3 Center {get; set;}
        public VertexReferenceFlags Flag;
        private readonly VertexData[]? ParentedVertices;
        private VertexData v0 => ParentedVertices![0];
        private VertexData v1 => ParentedVertices![1];
        public VertexData(Vector3 position/*, Vector3 normal, Vector3[] otherTwoVertex*/)
        {
            m_pos = Vector3.Round(position);
            IsAbsoluteVertex = true;
            //_normal = normal;
        }
        public VertexData(VertexData referencedVertex0, VertexData referencedVertex1, VertexReferenceFlags flag)
        {
            ParentedVertices = new VertexData[2];
            ParentedVertices[0] = referencedVertex0;
            ParentedVertices[1] = referencedVertex1;
            IsAbsoluteVertex = false;
            Flag = flag;
        }
        public float[] GetTextureCoordinates()
        {
            float[] texCoordinates = new float[2];
            Vector3 pos = Position;
            // var pX = pos.X - Center.X;
            // var pY = pos.Y - Center.Y;
            // var pZ = pos.Z - Center.Z;
            // if (!MathTeacher.Within(-1, pX, 1))
            //     pX -= pX.GetNextBiggerRoundedNumber();
            // if (!MathTeacher.Within(-1, pZ, 1))
            //     pZ -= pZ.GetNextBiggerRoundedNumber();
            // var lY = MathF.Asin(pZ);
            // var lX = MathF.Acos(pZ);

            // var y = float.Lerp(pos.Y, pos.Z, lY);
            // var x = float.Lerp(pos.X, pos.Z, lX);

            float texCoordX = m_useZtoX ? pos.Z : -pos.X;
            float texCoordY = m_useZtoY ? pos.Z : pos.Y;
            texCoordinates[0] = texCoordX * (m_flipX ? -1 : 1);
            texCoordinates[1] = texCoordY * (m_flipY ? -1 : 1);
            return texCoordinates;
        }

        //TODO: Add snap interval and check its value here
        internal void AddPosition(Vector3 addedPos) => Position += addedPos;
    }
}
