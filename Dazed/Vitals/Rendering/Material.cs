using OpenTK.Mathematics;

namespace Dazed.Vitals.Rendering
{
    public class Material
    {
        private readonly Shader _materialShader;
        private readonly Dictionary<string, int> _keyToIndex = [];
        private readonly List<MaterialParameter> _params = [];

        private Texture2D? m_mainTexture;

        internal event Action<int>? OnParameterChange;

        private static readonly Dictionary<string, int> Indexes = [];
        private static readonly List<Material> InstancedMaterials = [];

        public static Material MissingMaterial{get; private set;} = null!;
        internal static void CreateMissingMaterial(Shader shader, Texture2D texture)
        {
            MissingMaterial = new Material(shader);
            MissingMaterial.SetMainTexture(texture);
        }

        public static Material GetMaterialInstance(Shader matShader, bool litMaterial = false, bool forceNewInstance = false)
        {
            if (!forceNewInstance)
                if (Indexes.TryGetValue(matShader.ShaderIdentifier, out var index))
                    return InstancedMaterials[index];
            Material? m = null;
            m = new Material(matShader, litMaterial);
            Indexes.Add(matShader.ShaderIdentifier, InstancedMaterials.Count);
            InstancedMaterials.Add(m);
            return m;
        }

        public static Material GetMaterialInstance(SerializedMaterialData data, bool forceNewInstance = false)
        {
            if (!forceNewInstance)
                if (Indexes.TryGetValue(data.ShaderIdentification, out var index))
                    return InstancedMaterials[index];
            Material? m = null;
            m = new Material(Shader.GetNewShaderInstance(data.VertexShaderRelativePath, data.FragmentShaderRelativePath), data.IsLit);
            Indexes.Add(data.ShaderIdentification, InstancedMaterials.Count);
            InstancedMaterials.Add(m);
            return m;
        }

        public void UseMaterial()
        {
            UpdateShaderParameters(true);
            m_mainTexture?.Use(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);
        }

        private void SetMainTexture(Texture2D texture)
        {
            m_mainTexture = texture;
        }

        public void SetFloat(string name, float value)
        {
            try
            {
                SetValue(name, value);
                OnParameterChange?.Invoke(ParameterLocation(name));
            }
            catch (Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }

        public void SetVector2(string name, Vector2 value)
        {
            try
            {
                SetValue(name, value);
                OnParameterChange?.Invoke(ParameterLocation(name));
            }
            catch (Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }

        public void SetVector3(string name, Vector3 value)
        {
            try
            {
                SetValue(name, value);
                OnParameterChange?.Invoke(ParameterLocation(name));
            }
            catch (Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }
        public void SetVector3(string name, System.Numerics.Vector3 value) => SetVector3(name, (Vector3)value);
        public void SetVector4(string name, Vector4 value)
        {
            try
            {
                SetValue(name, value);
                OnParameterChange?.Invoke(ParameterLocation(name));
            }
            catch (Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }
        public void SetVector4(string name, System.Numerics.Vector4 value) => SetVector4(name, (Vector4)value);
        public void SetVector4(string name, Color4 value)
        {
            try
            {
                SetValue(name, value);
                OnParameterChange?.Invoke(ParameterLocation(name));
            }
            catch (Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }
        public void SetMatrix4(string name, Matrix4 value)
        {
            try
            {
                SetValue(name, value);
                OnParameterChange?.Invoke(ParameterLocation(name));
            }
            catch (Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }
        public void SetMatrix4(string name, System.Numerics.Matrix4x4 value) => SetMatrix4(name, (Matrix4)value);
        public void SetBool(string name, bool value)
        {
            try
            {
                SetValue(name, value);
                OnParameterChange?.Invoke(ParameterLocation(name));
            }
            catch (Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }

        private void UpdateShaderParameters()
        {
            _params.Clear();
            _keyToIndex.Clear();
            GetUniformsFromShader();
            UpdateShaderParameters(true);
        }

        private void UpdateShaderParameters(bool forceReload = false)
        {
            for(int i = 0; i < _params.Count; i++)
            {
                if (!forceReload && !_params[i].Changed)
                    continue;
                _materialShader.SetInterpretedValue(_params[i].Name, _params[i].Value);
                _params[i].UnflagChange();
            }
        }

        private int ParameterLocation(string name) => _keyToIndex[name];
        private void SetValue<T>(string name, T value) where T : struct => _params[ParameterLocation(name)].SetValue(value);

        private Material(Shader shader, bool litMaterial = false)
        {
            _materialShader = shader;
            if (litMaterial)
                Artist.AddLitMaterial(this);
            Shader.AfterShaderReload += UpdateShaderParameters;
            GetUniformsFromShader();
        }

        private void GetUniformsFromShader()
        {
            string[] keys = _materialShader.GetUniformsKeys();
            _params.Capacity = keys.Length;
            for(int i = 0; i < keys.Length; i++)
            {
                _params.Add(new(keys[i], default));
                _keyToIndex.Add(keys[i], i);
            }
        }

        public int GetAttribLocation(string v) => _materialShader.GetAttribLocation(v);
    }
}
