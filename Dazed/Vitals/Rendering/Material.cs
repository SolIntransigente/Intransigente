using Newtonsoft.Json.Linq;
using System.Numerics;

namespace Dazed.Vitals.Rendering
{
    public class Material
    {
        private readonly Shader m_shader;
        private readonly Dictionary<string, int> m_keyToIndex = [];
        private readonly List<MaterialParameter> m_params = [];

        private Texture2D? m_mainTexture;
        private string m_identifier;

        internal event Action<int>? OnParameterChange;

        private static readonly Dictionary<string, int> Indexes = [];
        private static readonly List<Material> InstancedMaterials = [];

        public static Material MissingMaterial{get; private set;} = null!;
        public string ShaderIdentification { get; private set; }

        internal static void CreateMissingMaterial(Shader shader, Texture2D texture)
        {
            MissingMaterial = new Material(shader);
            MissingMaterial.SetMainTexture(texture);
        }

        public static void TryRemoveInstance(Material matInstance)
        {
            if (matInstance == null)
                return;
            if (!Indexes.ContainsKey(matInstance.ShaderIdentification))
                return;
            Indexes.Remove(matInstance.ShaderIdentification);
            InstancedMaterials.Remove(matInstance);   
        }

        public static Material GetMaterialInstance(Shader matShader, bool litMaterial = false, bool forceNewInstance = false)
        {
            if (!forceNewInstance)
                if (Indexes.TryGetValue(matShader.ShaderIdentifier, out var index))
                    return InstancedMaterials[index];
            Material? m = null;
            m = new Material(matShader, isLit: litMaterial);
            Indexes.Add(matShader.ShaderIdentifier, InstancedMaterials.Count);
            InstancedMaterials.Add(m);
            return m;
        }

        public static Material GetMaterialInstance(string pathToMaterialData, bool forceNewInstance = false) => GetMaterialInstance(Cereal.LoadData<SerializedMaterialData>(pathToMaterialData), forceNewInstance);
        public static Material GetMaterialInstance(SerializedMaterialData data, bool forceNewInstance = false)
        {
            if (!forceNewInstance)
                if (Indexes.TryGetValue(data.ShaderIdentification, out var index))
                    return InstancedMaterials[index];
            Material? m = null;
            m = new Material(Shader.GetNewShaderInstance(data.VertexShaderRelativePath, data.FragmentShaderRelativePath), data.MaterialName, data.IsLit);
            m.SetData(data);
            m.ShaderIdentification = data.ShaderIdentification;
            Indexes.Add(data.ShaderIdentification, InstancedMaterials.Count);
            InstancedMaterials.Add(m);
            return m;
        }

        private void SetData(SerializedMaterialData data)
        {
            for(int i = 0; i < data.Properties.Length; i++)
            {
                if (!m_shader.HasUniform(data.Properties[i].UniformName))
                {
                    Scriptorium.Warn($"Material '{m_identifier}' does not have the uniform '{data.Properties[i].UniformName}' so this property was skipped");
                    continue;
                }

                m_shader.SetInterpretedValue(data.Properties[i].UniformName, GetProperValue(data.Properties[i].UniformValue, data.Properties[i].UniformType));
            }
        }

        private object? GetProperValue(object? uniformValue, string typeName)
        {
            if (uniformValue == null)
                return null;
            Type uniformType = Shader.GetTypeFor(typeName);
            if (uniformValue.GetType() == typeof(JObject))
            {
                object? v = ((JObject)uniformValue).ToObject(uniformType);
                if (uniformType == typeof(TexturePath))
                {
                    SetMainTexture((Texture2D?)ProcessAsTexture(v));
                    return null;
                }
                return v;
            }
            else return uniformValue;
        }

        private Texture? ProcessAsTexture(object? v)
        {
            if (v == null)
                return null;
            try
            {
                TexturePath tPath = (TexturePath)v;
                return Texture.GetTextureFor(tPath.Identification);
            }
            catch (Exception ex)
            {
                Scriptorium.LogException(ex);
                return null;
            }
        }

        public void UseMaterial()
        {
            m_shader.Use();
            UpdateShaderParameters(false);
            m_mainTexture?.Use(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);
        }

        public void SetMainTexture(Texture2D? texture)
        {
            m_mainTexture = texture ?? Artist.MissingTexture;
        }

        public void SetFloat(string name, float value)
        {
            try
            {
                SetValueInternal(name, value);
            }
            catch (Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }
        public void TrySetFloat(string name, float value)
        {
            try
            {
                SetValueInternal(name, value);
            }
            catch {return;}
        }

        public void SetVector2(string name, Vector2 value)
        {
            try
            {
                SetValueInternal(name, value);
            }
            catch (Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }
        public void TrySetVector2(string name, Vector2 value)
        {
            try
            {
                SetValueInternal(name, value);
            }
            catch {return;}
        }

        public void SetVector3(string name, Vector3 value)
        {
            try
            {
                SetValueInternal(name, value);
            }
            catch (Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }
        public void TrySetVector3(string name, Vector3 value)
        {
            try
            {
                SetValueInternal(name, value);
            }
            catch
            {return;}
        }
        public void SetVector4(string name, Vector4 value)
        {
            try
            {
                SetValueInternal(name, value);
            }
            catch (Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }
        public void SetVector4(string name, OpenTK.Mathematics.Color4 value)
        {
            try
            {
                SetValueInternal(name, value);
            }
            catch (Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }
        public void TrySetVector4(string name, OpenTK.Mathematics.Color4 value)
        {
            try
            {
                SetValueInternal(name, value);
            }
            catch {return;}
        }
        public void TrySetVector4(string name, Vector4 value)
        {
            try
            {
                SetValueInternal(name, value);
            }
            catch {return;}
        }
        public void SetVector4n(string name, System.Numerics.Vector4 value) => SetVector4(name, (Vector4)value);
        public void TrySetVector4n(string name, System.Numerics.Vector4 value) => TrySetVector4(name, (Vector4)value);

        public void SetMatrix4(string name, Matrix4x4 value)
        {
            try
            {
                SetValueInternal(name, value);
            }
            catch (Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }
        public void TrySetMatrix4(string name, Matrix4x4 value)
        {
            try
            {
                SetValueInternal(name, value);
            }
            catch {return;}
        }
        public void SetBool(string name, bool value)
        {
            try
            {
                SetValueInternal(name, value);
            }
            catch (Exception ex)
            {
                Scriptorium.LogException(ex);
            }
        }
        public void TrySetBool(string name, bool value)
        {
            if (DebugState.DebugFull)
            {
                SetBool(name, value);
                return;
            }
            try
            {
                SetValueInternal(name, value);
            }
            catch{return;}
        }

        private void SetValueInternal<T>(string name, T value) where T : struct
        {
            SetValue<T>(name, value);
            OnParameterChange?.Invoke(ParameterLocation(name));
        }

        private void UpdateShaderParameters(bool forceAll)
        {
            for (int i = 0; i < m_params.Count; i++)
            {
                if (!forceAll && !m_params[i].Changed)
                    continue;
                m_shader.SetInterpretedValue(m_params[i].Name, m_params[i].Value);
                m_params[i].UnflagChange();
            }
        }

        private void ReloadAllParameters() => ReloadAllParameters(true);

        public void ReloadAllParameters(bool reloadAllParameters = false)
        {
            m_params.Clear();
            m_keyToIndex.Clear();
            GetUniformsFromShader();
            UpdateShaderParameters(reloadAllParameters);
        }

        private int ParameterLocation(string name) => m_keyToIndex[name];
        private void SetValue<T>(string name, T value) where T : struct
        {
            m_params[ParameterLocation(name)].SetValue(value);
        }

        private Material(Shader shader, string? identifier = null, bool isLit = false)
        {
            m_shader = shader;
            m_identifier = identifier ?? "UNDEFINED MATERIAL";
            if (isLit)
                Artist.AddLitMaterial(this);
            Shader.AfterShaderReload += ReloadAllParameters;
            GetUniformsFromShader();
        }

        private void GetUniformsFromShader()
        {
            string[] keys = m_shader.GetUniformsKeys();
            m_params.Capacity = keys.Length;
            for(int i = 0; i < keys.Length; i++)
            {
                m_params.Add(new(keys[i], default));
                m_keyToIndex.Add(keys[i], i);
            }
        }

        public int GetAttribLocation(string v) => m_shader.GetAttribLocation(v);
    }
}
