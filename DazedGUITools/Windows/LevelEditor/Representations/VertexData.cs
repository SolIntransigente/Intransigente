using Newtonsoft.Json.Linq;
using System;
using System.Numerics;

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
                    return _pos;
                else
                {
                    Vector3 value = new();
                    if (_flag!.Value.useX0)
                        value.X = v0.Position.X;
                    else
                        value.X = v1.Position.X;
                    if (_flag!.Value.useY0)
                        value.Y = v0.Position.Y;
                    else
                        value.Y = v1.Position.Y;
                    if (_flag!.Value.useZ0)
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
                    _pos = value;
                }
                else
                {
                    if(_flag!.Value.useX0 || _flag.Value.useY0 || _flag.Value.useZ0)
                    {
                        Vector3 pos0 = new Vector3(_flag.Value.useX0 ? value.X : v0.Position.X, _flag.Value.useY0 ? value.Y : v0.Position.Y, _flag.Value.useZ0 ? value.Z : v0.Position.Z);
                        ParentedVertices[0].Position = pos0;
                    }
                    if(!_flag.Value.useX0 || !_flag.Value.useY0 || !_flag.Value.useZ0)
                    {
                        Vector3 pos1 = new Vector3(!_flag.Value.useX0 ? value.X : v1.Position.X, !_flag.Value.useY0 ? value.Y : v1.Position.Y, !_flag.Value.useZ0 ? value.Z : v1.Position.Z);
                        ParentedVertices[1].Position = pos1;
                    }
                }
            }
        }
        public Vector3 Normal
        {
            get => ParentedVertices == null ? _normal : v0.Normal;
            internal set
            {
                if (ParentedVertices != null)
                    return;
                _normal = value;
            }
        }
        private Vector3 _normal;
        private Vector3 _pos;
        private VertexReferenceFlags? _flag;
        private readonly VertexData[]? ParentedVertices;
        private VertexData v0 => ParentedVertices![0];
        private VertexData v1 => ParentedVertices![1];
        public VertexData(Vector3 position/*, Vector3 normal, Vector3[] otherTwoVertex*/)
        {
            _pos = Vector3.Round(position);
            IsAbsoluteVertex = true;
            //_normal = normal;
        }
        public VertexData(VertexData referencedVertex0, VertexData referencedVertex1, VertexReferenceFlags flag)
        {
            ParentedVertices = new VertexData[2];
            ParentedVertices[0] = referencedVertex0;
            ParentedVertices[1] = referencedVertex1;
            IsAbsoluteVertex = false;
            _flag = flag;
        }
        public float[] GetTextureCoordinates()
        {
            float[] texCoordinates = new float[2];
            Vector3 pos = Position;
            var c1 = new Vector2(pos.Y, pos.Z);
            var c2 = new Vector2(pos.X, pos.Z);
            var c3 = new Vector2(pos.X, pos.Y);

            var alpha21 = MathF.Abs(Normal.X);
            var alpha23 = MathF.Abs(Normal.Z);

            var c21 = Vector2.Lerp(c2, c1, alpha21);
            var c23 = Vector2.Lerp(c21, c3, alpha23);
            texCoordinates[0] = c23.X;
            texCoordinates[1] = c23.Y;
            return texCoordinates;
        }

        //TODO: Add snap interval and check its value here
        internal void AddPosition(Vector3 addedPos) => Position += addedPos;
    }
}
