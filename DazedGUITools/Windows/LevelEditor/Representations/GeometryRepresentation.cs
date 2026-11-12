using Dazed.Vitals;
using Dazed.Vitals.Rendering;
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;
using System.Numerics;

namespace DazedGUITools.Windows.LevelEditor.Representations
{
    internal class GeometryRepresentation
    {
        public SectionRepresentation ParentedSection {get; set;}

        public int VertexCount {get; private init;}

        private int _vertexArrayObject, _elementBufferObject, _vertexBufferObject;
        private readonly VertexData[] m_vertData;
        private Vector3 m_center;
        private readonly ushort[] m_indices;

        public string m_materialPath; 
        private Material GeometryMaterial;

        /// <summary>
        /// Constructs a quad geometry by taking the max and min vertex - Order doesn't matter
        /// </summary>
        /// <returns>A new quad</returns>
        public static GeometryRepresentation QuadGeometry(Vector3 v0, Vector3 v1)
        {
            var min = Vector3.Min(v0, v1);
            var max = Vector3.Max(v1, v0);

            var minVert = new VertexData(min);
            var maxVert = new VertexData(max);
            var topLeft = new VertexData(minVert, maxVert, new(true, false, false));
            var bottomRight = new VertexData(minVert, maxVert, new(false, true, true));

            var n = Vector3.Cross(max - min, topLeft.Position - min);
            var normal = Vector3.Normalize(n);
            minVert.Normal = normal;
            maxVert.Normal = normal;

            return new GeometryRepresentation([maxVert, bottomRight, minVert, topLeft], [/*First triangle: */0, 1, 3, /*The other triangle:*/ 1, 2, 3]);
        }

        private GeometryRepresentation(VertexData[] vertexData, ushort[] indices)
        {
            if (vertexData.Length < 3)
                throw new ArgumentOutOfRangeException(nameof(vertexData), "Vertex count must be at least 3");
            m_vertData = vertexData;
            VertexCount = vertexData.Length;
            m_indices = indices;
            m_center = new();
            ParentedSection = LevelEditorContext.CurrentlySelectedSection;
            m_materialPath = Path.Combine(MainWindow.ProjectResourcesFolder, CommonPaths.MaterialFolderName);
            m_materialPath = Path.Combine(m_materialPath, "default" + CommonPaths.MaterialExtension);
            GeometryMaterial = Material.GetMaterialInstance(m_materialPath);
            UpdateData();
        }

        private void UpdateData()
        {
            SetUpData();
            UpdateBuffers();
        }

        private void UpdateBuffers()
        {
            float[] vertData = new float[m_vertData.Length * 3 + m_vertData.Length * 2];
            var realIndex = 0;
            for (int i = 0; i < vertData.Length;)
            {
                var offset = 0;
                Vector3 pos = m_vertData[realIndex].Position;
                float[] texCoord = m_vertData[realIndex].GetTextureCoordinates();
                vertData[i + offset++] = pos.X;
                vertData[i + offset++] = pos.Y;
                vertData[i + offset++] = pos.Z;
                /*--------------Texture coordinates---------------*/
                vertData[i + offset++] = texCoord[0];
                vertData[i + offset++] = texCoord[1];
                i += offset;
                realIndex++;
            }
            if (_vertexBufferObject == 0)
                _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertData.Length * sizeof(float), vertData, BufferUsageHint.DynamicDraw);

            if (_elementBufferObject == 0)
                _elementBufferObject = GL.GenBuffer();
            if (_vertexArrayObject == 0)
                _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            SetAttributes();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, m_indices.Length * sizeof(ushort), m_indices, BufferUsageHint.DynamicDraw);

            GL.BindVertexArray(0);
        }

        private void SetAttributes()
        {
            int positionLocation = GeometryMaterial.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            int textCoord = GeometryMaterial.GetAttribLocation("aTexCoords");
            GL.EnableVertexAttribArray(textCoord);
            GL.VertexAttribPointer(textCoord, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        }

        private void SetUpData()
        {
            for (int i = 0; i < m_vertData.Length; i++)
                m_center += m_vertData[i].Position;

            m_center /= m_vertData.Length;

            var v0 = m_vertData[0].Position;
            var v1 = m_vertData[1].Position;
            var v2 = m_vertData[2].Position;
            var n = Vector3.Cross(v0 - v1, v2 - v1);
            var normal = Vector3.Normalize(n);
            for(int i = 0; i < m_vertData.Length; i++)
                m_vertData[i].Normal = normal;
        }

        internal void Draw()
        {
            GL.BindVertexArray(_vertexArrayObject);
            GeometryMaterial.SetMatrix4("model", Matrix4x4.Identity);
            GeometryMaterial.SetMatrix4("projection", LevelEditorCamera.Projection);
            GeometryMaterial.SetMatrix4("view", LevelEditorCamera.View);
            GeometryMaterial.UseMaterial();
            GL.DrawElements(PrimitiveType.Triangles, m_indices.Length, DrawElementsType.UnsignedShort, 0);
            GL.BindVertexArray(0);
        }

        internal void MoveVertex(int index, Vector3 toPosition)
        {
            if (index > m_vertData.Length)
                return;
            if (index < 0)
                return;
            m_vertData[index].Position = toPosition;
            UpdateData();
        }

        internal VertexData VertexAt(int index)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, m_vertData.Length);
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            return m_vertData[index];
        }

        internal void AddPosition(Vector3 addedPos)
        {
            for(short i = 0; i < m_vertData.Length; i++)
            {
                if (!m_vertData[i].IsAbsoluteVertex)
                    continue;
                m_vertData[i].AddPosition(addedPos);
            }
            UpdateData();
        }

        internal void AddPosition(int index, Vector3 addedPos)
        {
            var vert = VertexAt(index);
            if (vert == null)
                return;
            for(short i = 0; i < m_vertData.Length; i++)
                if (i == index)
                    continue;
                else if(vert.Position + addedPos == m_vertData[i].Position)
                    return;
            vert.AddPosition(addedPos);
            UpdateData();
        }

        internal void FlipNormal()
        {
            ushort[] newIndex = new ushort[m_indices.Length];
            for (ushort i = 0; i < newIndex.Length; i++)
                newIndex[i] = m_indices[m_indices.Length - i - 1];
            for (ushort i = 0; i < newIndex.Length; i++)
                m_indices[i] = newIndex[i];
            UpdateData();
        }

        internal void SetPosition(int index, Vector3 toPosition)
        {
            var vert = VertexAt(index);
            if (vert == null)
                return;
            for(short i = 0; i < m_vertData.Length; i++)
                if (i == index)
                    continue;
                else if(toPosition == m_vertData[i].Position)
                    return;
            vert.Position = toPosition;
            UpdateData();
        }

        public bool TrySetMaterial(string path, out string message) 
        {
            message = string.Empty;
            if (!path.EndsWith(CommonPaths.MaterialExtension)) 
            {
                message = $"File is not material, or does not have the appropiate extesion '{CommonPaths.MaterialExtension}'";
                return false;
            }
            var data = Cereal.SafeLoadData<SerializedMaterialData>(path, out bool successful);
            if (!successful) 
            {
                message = "File failed at being deserialized";
                return false;
            }
            try 
            { 
                var matInstance = Material.GetMaterialInstance(data);
                GeometryMaterial = matInstance;
            }
            catch (Exception ex) 
            {
                message = ex.Message;
                return false;
            }
            m_materialPath = path;
            return true;
        }

        internal void ForceUpdate()
        {
            UpdateData();
        }
    }
}
