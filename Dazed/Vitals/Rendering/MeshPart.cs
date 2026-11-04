using OpenTK.Graphics.OpenGL;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dazed.Vitals.Rendering
{
    public class MeshPart : Renderable
    {
        private readonly ushort m_indicesCount;
        private readonly ContainedMeshData m_containedMeshData;
        public Material? PartMaterial
        {
            get => m_partMaterial;
            set
            {
                m_partMaterial = value;
                ReloadMaterialData();
            }
        }
        private Material? m_partMaterial;
        private Matrix4x4 m_meshMatrix;
        internal Matrix4x4 m_worldMatrix;

        private readonly int _vertexArrayObject;
        private const int ByteSizeOfHalf = 2;

        internal MeshPart(float[] vertPos, float[]? normals, float[]? textureCoordinates, uint[] indices, int vertexCount, Matrix4x4 localMeshMatrix, Material material)
        {
            if (vertPos == null || vertPos.Length == 0)
                throw new ArgumentException("Vertex positions array can NOT be null nor empty.");

            m_meshMatrix = localMeshMatrix;
            m_containedMeshData = ContainedMeshData.position;
            if(normals != null)
                m_containedMeshData = (ContainedMeshData)(int)(ContainedMeshData.normals + (int)m_containedMeshData);
            if (textureCoordinates != null)
                m_containedMeshData = (ContainedMeshData)(int)(ContainedMeshData.textureCoordinates + (int)m_containedMeshData);

            Half[] arrayBufferData = GetProperArray(ref vertPos, ref normals!, ref textureCoordinates!);

            m_indicesCount = (ushort) indices.Length;

            VertexCount = vertexCount;
            var vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferStorage(BufferTarget.ArrayBuffer, arrayBufferData.Length * ByteSizeOfHalf, arrayBufferData, BufferStorageFlags.None);

            var elementBufferObject = GL.GenBuffer();
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            PartMaterial = material;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferStorage(BufferTarget.ElementArrayBuffer, m_indicesCount * sizeof(uint), indices, BufferStorageFlags.None);

            GL.BindVertexArray(0);
        }

        private Half[] GetProperArray(ref float[] vertPos, ref float[] normals, ref float[] textureCoordinates)
        {
            switch (m_containedMeshData)
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
            if (m_partMaterial == null)
            {
                if(DebugState.DebugLow)
                    Scriptorium.Warn("A mesh can not be drawn because it has no shader");
                return;
            }
            // SetRenderingValues(in data, WorldMatrix);
            Artist.SetCommonValues(m_partMaterial, in data, ref m_worldMatrix, ref m_meshMatrix);
            GL.BindVertexArray(_vertexArrayObject);
            m_partMaterial.UseMaterial();
            GL.DrawElements(PrimitiveType.Triangles, m_indicesCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
        
        private void ReloadMaterialData()
        {
            if (m_partMaterial == null)
                return;
            GL.BindVertexArray(_vertexArrayObject);
            switch (m_containedMeshData)
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
            int positionLocation = m_partMaterial!.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.HalfFloat, false, 8 * ByteSizeOfHalf, 0);

            int normalLocation = m_partMaterial.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.HalfFloat, false, 8 * ByteSizeOfHalf, 3 * ByteSizeOfHalf);

            int textCoord = m_partMaterial.GetAttribLocation("aTexCoords");
            GL.EnableVertexAttribArray(textCoord);
            GL.VertexAttribPointer(textCoord, 2, VertexAttribPointerType.HalfFloat, false, 8 * ByteSizeOfHalf, 6 * ByteSizeOfHalf);
        }

        private void PositionAndTextureCoordinatesAttributePointer()
        {
            int positionLocation = m_partMaterial!.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.HalfFloat, false, 5 * ByteSizeOfHalf, 0);

            int textCoord = m_partMaterial.GetAttribLocation("aTexCoords");
            GL.EnableVertexAttribArray(textCoord);
            GL.VertexAttribPointer(textCoord, 2, VertexAttribPointerType.HalfFloat, false, 5 * ByteSizeOfHalf, 3 * ByteSizeOfHalf);
        }

        private void PositionAndNormalAttributePointer()
        {
            int positionLocation = m_partMaterial!.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.HalfFloat, false, 6 * ByteSizeOfHalf, 0);

            int normalLocation = m_partMaterial.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.HalfFloat, false, 6 * ByteSizeOfHalf, 3 * ByteSizeOfHalf);
        }

        private void PositionAttributePointer()
        {
            int positionLocation = m_partMaterial!.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.HalfFloat, false, 3 * ByteSizeOfHalf, 0);
        }
        public int VertexCount { get; }

        public override bool AllowRendering => true;
    }
}
