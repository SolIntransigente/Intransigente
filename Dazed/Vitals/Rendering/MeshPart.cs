using OpenTK.Graphics.OpenGL;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dazed.Vitals.Rendering
{
    public class MeshPart : Renderable
    {
        private readonly uint[] _indices;
        private readonly ContainedMeshData _containedMeshData;
        public Material? PartMaterial
        {
            get => _partMaterial;
            set
            {
                _partMaterial = value;
                ReloadMaterialData();
            }
        }
        private Material? _partMaterial;
        private readonly Matrix4x4 _meshMatrix;
        internal Matrix4x4 WorldMatrix { get; set; }

        private readonly int _vertexArrayObject;
        private const int ByteSizeOfHalf = 2;

        internal MeshPart(float[] vertPos, float[]? normals, float[]? textureCoordinates, uint[] indices, int vertexCount, Matrix4x4 localMeshMatrix, Material material)
        {
            if (vertPos == null || vertPos.Length == 0)
                throw new ArgumentException("Vertex positions array can NOT be null nor empty.");

            _meshMatrix = localMeshMatrix;
            _containedMeshData = ContainedMeshData.position;
            if(normals != null)
                _containedMeshData = (ContainedMeshData)(int)(ContainedMeshData.normals + (int)_containedMeshData);
            if (textureCoordinates != null)
                _containedMeshData = (ContainedMeshData)(int)(ContainedMeshData.textureCoordinates + (int)_containedMeshData);

            Half[] arrayBufferData = GetProperArray(ref vertPos, ref normals, ref textureCoordinates);

            _indices = indices;

            VertexCount = vertexCount;
            var vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferStorage(BufferTarget.ArrayBuffer, arrayBufferData.Length * ByteSizeOfHalf, arrayBufferData, BufferStorageFlags.None);
            //GL.BufferData(BufferTarget.ArrayBuffer, arrayBufferData.Length * ByteSizeOfHalf, arrayBufferData, BufferUsageHint.StaticDraw);

            var elementBufferObject = GL.GenBuffer();
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            PartMaterial = material;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferStorage(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferStorageFlags.None);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
        }

        private Half[] GetProperArray(ref float[] vertPos, ref float[] normals, ref float[] textureCoordinates)
        {
            switch (_containedMeshData)
            {
                case ContainedMeshData.position:
                    Half[] val = new Half[vertPos.Length];
                    for (int i = 0; i < val.Length; i++)
                        val[i] = (Half)vertPos[i];
                    return val;
                case ContainedMeshData.positionAndNormals:
                    return ArrangePosAndNormals(ref vertPos, ref normals);
                case ContainedMeshData.positionAndTextureCoordinates:
                    return ArrangePosAndTextureCoordinates(ref vertPos, ref textureCoordinates);
                default:
                    return ArrangeAll(ref vertPos, ref normals, ref textureCoordinates);
            }
        }

        private Half[] ArrangeAll(ref float[] vertPos, ref float[] normals, ref float[] textureCoordinates)
        {
            int accumulated = 0;
            int offset = 0;

            var array = new Half[vertPos.Length + normals.Length + textureCoordinates.Length];
            for (int v = 0; v < vertPos.Length; v++)
            {
                array[v + offset] = (Half)vertPos[v];
                accumulated += 1;
                if (accumulated == 3)
                {
                    //3 - As normals are X,Y,Z
                    //+2 - As texture coordinates are X,Y
                    offset += 3 + 2;
                    accumulated = 0;
                }
            }
            offset = 3;
            accumulated = 0;
            for (int n = 0; n < normals.Length; n++)
            {
                array[n + offset] = (Half)normals[n];
                accumulated += 1;
                if (accumulated == 3)
                {
                    //3 - As vertex position is X,Y,Z
                    //+2 - As texture coords are X,Y,Z
                    offset += 3 + 2;
                    accumulated = 0;
                }
            }
            offset = 5;
            accumulated = 0;
            for(int t = 0; t < textureCoordinates.Length; t++)
            {
                array[t + offset] = (Half)textureCoordinates[t];
                accumulated++;
                if(accumulated == 2)
                {
                    //3 - As vertex position is X,Y,Z
                    //+3 - As normals are X,Y,Z
                    offset += 3 + 3;
                    accumulated = 0;
                }
            }
            return array;
        }

        private Half[] ArrangePosAndTextureCoordinates(ref float[] vertPos, ref float[] textureCoordinates)
        {
            int accumulated = 0;
            int offset = 0;

            var array = new Half[vertPos.Length + textureCoordinates.Length];
            for (int v = 0; v < vertPos.Length; v++)
            {
                array[v + offset] = (Half)vertPos[v];
                accumulated += 1;
                if (accumulated == 3)
                {
                    //2 - As texture coordinates are X,Y
                    offset += 2;
                    accumulated = 0;
                }
            }
            offset = 2;
            for (int n = 0; n < textureCoordinates.Length; n++)
            {
                array[n + offset] = (Half)textureCoordinates[n];
                accumulated += 1;
                if (accumulated == 2)
                {
                    //3 - As vertex position is X,Y,Z
                    offset += 3;
                    accumulated = 0;
                }
            }
            return array;
        }

        private Half[] ArrangePosAndNormals(ref float[] vertPos, ref float[] normals)
        {
            int accumulated = 0;
            int offset = 0;

            var array = new Half[vertPos.Length + normals.Length];
            for (int v = 0; v < vertPos.Length; v++)
            {
                array[v + offset] = (Half)vertPos[v];
                accumulated += 1;
                if (accumulated == 3)
                {
                    //3 - As normals are X,Y,Z
                    offset += 3;
                    accumulated = 0;
                }
            }
            offset = 3;
            for (int n = 0; n < normals.Length; n++)
            {
                array[n + offset] = (Half)normals[n];
                accumulated += 1;
                if (accumulated == 3)
                {
                    //3 - As vertex position is X,Y,Z
                    offset += 3;
                    accumulated = 0;
                }
            }
            return array;
        }

        public override void Render(in RenderingData data)
        {
            if (_partMaterial == null)
            {
                if(DebugState.DebugLow)
                    Scriptorium.Warn("A mesh can not be drawn because it has no shader");
                return;
            }
            SetRenderingValues(in data, WorldMatrix);
            GL.BindVertexArray(_vertexArrayObject);
            _partMaterial.UseMaterial();
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        private void SetRenderingValues(in RenderingData data, Matrix4x4 meshWorld)
        {
            Artist.MissingTexture.Use();
            if (_partMaterial == null) return;
            _partMaterial.SetMatrix4("view", data.ViewMatrix);
            _partMaterial.SetMatrix4("projection", data.ProjectionMatrix);
            _partMaterial.SetMatrix4("model", _meshMatrix * meshWorld);

            //TODO: Replace this section with something fancier and general
            _partMaterial.SetVector3("objectColor", new Vector3(1f, 1f, 1f));
            _partMaterial.SetVector3("ambientColor", new Vector3(0.1f, 0.1f, 0));
        }

        private void ReloadMaterialData()
        {
            if (_partMaterial == null)
                return;
            GL.BindVertexArray(_vertexArrayObject);
            switch (_containedMeshData)
            {
                case ContainedMeshData.position:
                    PositionAttributePointer();
                    break;
                case ContainedMeshData.positionAndNormals:
                    PositionAndNormalAttributePointer();
                    break;
                case ContainedMeshData.positionAndTextureCoordinates:
                    PositionAndTextureCoordinatesAttributePointer();
                    break;
                case ContainedMeshData.all:
                    AllAttributePointer();
                    break;
            }
        }

        private void AllAttributePointer()
        {
            int positionLocation = _partMaterial.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.HalfFloat, false, 8 * ByteSizeOfHalf, 0);

            int normalLocation = _partMaterial.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.HalfFloat, false, 8 * ByteSizeOfHalf, 3 * ByteSizeOfHalf);

            int textCoord = _partMaterial.GetAttribLocation("aTexCoords");
            GL.EnableVertexAttribArray(textCoord);
            GL.VertexAttribPointer(textCoord, 2, VertexAttribPointerType.HalfFloat, false, 8 * ByteSizeOfHalf, 6 * ByteSizeOfHalf);
        }

        private void PositionAndTextureCoordinatesAttributePointer()
        {
            int positionLocation = _partMaterial.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.HalfFloat, false, 5 * ByteSizeOfHalf, 0);

            int textCoord = _partMaterial.GetAttribLocation("aTexCoords");
            GL.EnableVertexAttribArray(textCoord);
            GL.VertexAttribPointer(textCoord, 2, VertexAttribPointerType.HalfFloat, false, 5 * ByteSizeOfHalf, 3 * ByteSizeOfHalf);
        }

        private void PositionAndNormalAttributePointer()
        {
            int positionLocation = _partMaterial.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.HalfFloat, false, 6 * ByteSizeOfHalf, 0);

            int normalLocation = _partMaterial.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.HalfFloat, false, 6 * ByteSizeOfHalf, 3 * ByteSizeOfHalf);
        }

        private void PositionAttributePointer()
        {
            int positionLocation = _partMaterial.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.HalfFloat, false, 3 * ByteSizeOfHalf, 0);
        }
        public int VertexCount { get; }

        public override bool AllowRendering => true;
    }
}
