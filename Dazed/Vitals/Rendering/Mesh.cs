using Dazed.Vitals.Content;
using Assimp;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace Dazed.Vitals.Rendering
{
    public class Mesh : Renderable
    {
        private readonly MeshPart[] _meshParts;

        public override bool AllowRendering => _allowRendering;
        private bool _allowRendering = true;

        private readonly uint _vertexCount;
        public uint VertexCount => _vertexCount;

        private Matrix4x4 _world;

        private static readonly Dictionary<string, Mesh> AllMeshInstance = [];
        public void ApplyRelativeToTransform(Matrix4x4 world) => _world = world;

        public void AllowForRendering(bool value) => _allowRendering = value; 

        public static Mesh GetMeshInstance(string path, Material material, MeshProperties? properties = null, bool pathIsAbsolute = false)
        {
            if (!pathIsAbsolute)
                path = PathUtils.BaseDirectoryCombine(path);
            if (AllMeshInstance.TryGetValue(path, out Mesh? m))
                return m;
            MeshProperties props = properties ?? MeshProperties.Default;
            m = new Mesh(path, material, props);
            AllMeshInstance.Add(path, m);
            return m;
        }

        public override void Render(in RenderingData data)
        {
            foreach (MeshPart part in _meshParts)
            {
                part.m_worldMatrix = _world;
                part.Render(data);
            }
        }

        private Mesh(string path, Material material, MeshProperties properties)
        {
            _vertexCount = 0;
            try
            {
                MeshPart[] allParts = LoadModelData(path, material, properties, out _vertexCount);
                _meshParts = allParts;
            }
            catch (Exception ex)
            {
                //TODO: Instead load an error model
                //! LoadModelData("the path to error model", <missing material>, <just use vertex data>);
                Scriptorium.LogException(ex);
                return;
            }
            // List<MeshPart> allParts = new();
            
        }

        private MeshPart[] LoadModelData(string path, Material material, MeshProperties properties, out uint vertexCount)
        {
            var meshData = Chest.AssimpContext.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices);
            MeshPart[] allParts = new MeshPart[meshData.Meshes.Count];
            vertexCount = 0;
            for (int i = 0; i < meshData.Meshes.Count; i++)
            {
                List<float> allVerts = new();
                List<float> allNormals = new();
                List<float> allTextureCoords = new();
                Assimp.Mesh mesh = meshData.Meshes[i];
                vertexCount += Convert.ToUInt32(mesh.VertexCount);
                for (int v = 0; v < mesh.Vertices.Count; v++)
                {
                    allVerts.Add(mesh.Vertices[v].X);
                    allVerts.Add(mesh.Vertices[v].Y);
                    allVerts.Add(mesh.Vertices[v].Z);
                    if (!properties.UseTextureCoords)
                        continue;
                    allTextureCoords.Add(mesh.TextureCoordinateChannels[0][v].X);
                    allTextureCoords.Add(mesh.TextureCoordinateChannels[0][v].Y);
                }
                if (properties.UseNormals)
                    for (int n = 0; n < mesh.Normals.Count; n++)
                    {
                        allNormals.Add(mesh.Normals[n].X);
                        allNormals.Add(mesh.Normals[n].Y);
                        allNormals.Add(mesh.Normals[n].Z);
                    }
                MeshPart part = new(allVerts.ToArray(), properties.UseNormals ? allNormals.ToArray() : null, properties.UseTextureCoords ? allTextureCoords.ToArray() : null, mesh.GetUnsignedIndices(), mesh.VertexCount,
                    MathTeacher.ToNumerics(meshData.RootNode.Children[i].Transform * meshData.RootNode.Transform), material);
                allParts[i] = part;
            }

            return allParts;
        }
    }
}
