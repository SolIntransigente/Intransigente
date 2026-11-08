using System.Numerics;
using Dazed.Vitals.FileHandling;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector4 = OpenTK.Mathematics.Vector4;

namespace Dazed.Vitals.Rendering
{
    public class Shader : IDisposable
    {
        private int _handle, _fragmentShader, _vertexShader;
        private bool _isDisposed;
        internal static event Action? ReloadAllShaders;
        internal static event Action? AfterShaderReload;

        private string _pathFrag, _pathVert;
        private readonly Dictionary<string, int> _uniformLocations = new();

        private readonly Dictionary<Type, Action<string, object>> ValueAssignments = [];

        public static Shader GetNewShaderInstance(string path, bool pathIsAbsolute = false)
        {
            string vertexPath = path + CommonPaths.VertexShaderFileExtension;
            string fragmentPath = path + CommonPaths.FragmentShaderFileExtension;
            if (!pathIsAbsolute)
            {
                vertexPath = Path.Combine(PathUtils.ShadersDirectory, vertexPath);
                fragmentPath = Path.Combine(PathUtils.ShadersDirectory, fragmentPath);
            }
            return GetShaderInstanceCommon(vertexPath, fragmentPath);
        }

        public static Shader GetNewShaderInstance(string vertPath, string fragPath, bool pathIsAbsolute = false)
        {
            if (!pathIsAbsolute)
            {
                vertPath = Path.Combine(PathUtils.ShadersDirectory, vertPath);
                fragPath = Path.Combine(PathUtils.ShadersDirectory, fragPath);
            }
            return GetShaderInstanceCommon(vertPath, fragPath);
        }

        private static Shader GetShaderInstanceCommon(string vertexPath, string fragmentPath)
        {
            if (!FilesExists(vertexPath, fragmentPath))
            {
                Scriptorium.Error($"One of the following paths do not exist: <{vertexPath}> || <{fragmentPath}>");
                throw new Exception();
            }
            Shader s = new(vertexPath, fragmentPath);
            return s;
        }

        private Shader(string vertPath, string fragPath)
        {
            InitializeWithPaths(vertPath, fragPath);
            SetUpValueAssignments();
            ReloadAllShaders += ReloadShader;
        }

        private void SetUpValueAssignments()
        {
            ValueAssignments.Add(typeof(float), SetFloat);
            ValueAssignments.Add(typeof(Vector2), SetVector2);
            ValueAssignments.Add(typeof(Vector3), SetVector3);
            ValueAssignments.Add(typeof(Vector4), SetVector4);
            ValueAssignments.Add(typeof(Color4), SetColor4);
            ValueAssignments.Add(typeof(Matrix4), SetMatrix4);
            ValueAssignments.Add(typeof(bool), SetBool);
        }

        internal string[] GetUniformsKeys() => _uniformLocations.Keys.ToArray();

        public static void Reload()
        {
            ReloadAllShaders?.Invoke();
            AfterShaderReload?.Invoke();
            Artist.ReloadScreenInfo();
        }

        private void ReloadShader() => InitializeWithPaths(_pathVert, _pathFrag);

        private void InitializeWithPaths(string vertFile, string fragFile)
        {
            //Assuming that if one is empty the other also is
            if (string.IsNullOrEmpty(_pathFrag))
            {
                _pathVert = Path.Combine(PathUtils.ShadersDirectory, vertFile);
                _pathFrag = Path.Combine(PathUtils.ShadersDirectory, fragFile);
            }
            var vertexShaderSource = Secretary.ReadContent(PathUtils.ShadersDirectory, _pathVert, filePathIsAbsolute : true);
            var fragmentShaderSource = Secretary.ReadContent(PathUtils.ShadersDirectory, _pathFrag, filePathIsAbsolute: true);

            if (string.IsNullOrEmpty(vertexShaderSource) || string.IsNullOrEmpty(vertexShaderSource))
                throw new Exception($"Either fragment shader at path <{_pathFrag}> or vertex shader at path <{_pathVert}> found an error while trying to be loaded.");

            _vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(_vertexShader, vertexShaderSource);
            if (!CompileShader(_vertexShader))
                if (_handle != 0)
                    return;
                else
                    throw new Exception("Error on vertex shader compilation, check command console for more info.");

            _fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(_fragmentShader, fragmentShaderSource);
            if (!CompileShader(_fragmentShader))
                if (_handle != 0)
                    return;
                else
                    throw new Exception("Error on fragment shader compilation, check command console for more info.");

            if (_handle == 0)
                _handle = GL.CreateProgram();

            GL.AttachShader(_handle, _vertexShader);
            GL.AttachShader(_handle, _fragmentShader);

            LinkProgram(_handle);

            GL.GetProgram(_handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            _uniformLocations.Clear();
            // Loop over all the uniforms,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // get the name of this uniform,
                var key = GL.GetActiveUniform(_handle, i, out _, out _);

                // get the location,
                var location = GL.GetUniformLocation(_handle, key);

                // and then add it to the dictionary.
                _uniformLocations.Add(key, location);
            }

            GL.DetachShader(_handle, _vertexShader);
            GL.DetachShader(_handle, _fragmentShader);
            GL.DeleteShader(_fragmentShader);
            GL.DeleteShader(_vertexShader);
        }

        private static bool FilesExists(string vertPath, string fragPath)
        {
            if (File.Exists(vertPath) && File.Exists(fragPath)) return true;
            if (!DebugState.DebugLow)
                return false;
            Scriptorium.Warn("Path for vertex / fragment shader does not exists");
            Scriptorium.Warn($"Vertex path: {vertPath}");
            Scriptorium.Warn($"Fragment path: {fragPath}");
            return false;

        }

        private static bool CompileShader(int shader)
        {
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
            return true;
        }
        private static void LinkProgram(int program)
        {
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
                throw new Exception($"Error occurred whilst linking Program({program})");
        }

        public void Use() => GL.UseProgram(_handle);

        public int GetAttribLocation(string attribName) => GL.GetAttribLocation(_handle, attribName);

        public void Dispose()
        {
            if (!_isDisposed)
                GL.DeleteProgram(_handle);
            _isDisposed = true;

            ReloadAllShaders -= ReloadShader;
            GC.SuppressFinalize(this);
        }

        internal void SetFloat(string name, object value)
        {
            Use();
            GL.Uniform1(_uniformLocations[name], (float)value);
        }

        internal void SetMatrix4(string name, object data)
        {
            Use();
            if (data.GetType() == typeof(Matrix4x4))
                data = MathTeacher.ToOpenTkMatrix4((Matrix4x4)data);
            var matrix4 = (Matrix4)data;
            GL.UniformMatrix4(_uniformLocations[name], true, ref matrix4);
        }

        internal void SetVector3(string name, object vector3)
        {
            Use();
            GL.Uniform3(_uniformLocations[name], (Vector3)vector3);
        }

        internal void SetVector2(string name, object vector2)
        {
            Use();
            GL.Uniform2(_uniformLocations[name], (Vector2)vector2);
        }

        internal void SetVector4(string name, object vector4)
        {
            Use();
            GL.Uniform4(_uniformLocations[name], (Vector4)vector4);
        }

        internal void SetColor4(string name, object color4)
        {
            Use();
            if(color4.GetType() == typeof(Color))
                color4 = (Color4)((Color)color4);
            GL.Uniform4(_uniformLocations[name], (Color4)color4);
        }

        internal void SetBool(string name, object value)
        {
            Use();
            GL.Uniform1(_uniformLocations[name], (bool)value ? 1 : 0);
        }

        internal void SetInterpretedValue(string name, object? value)
        {
            if (value == null)
            {
                if (DebugState.DebugFull) Scriptorium.Warn($"Null value returned for shader's uniform {name}");
                return;
            }
            Type type = value.GetType();
            if(ValueAssignments.TryGetValue(type, out var assignment))
            {
                assignment(name, value);
                return;
            }
            throw new ArgumentException($"Value {value} is not assignable to type {type}, or does not have an assignment function");
        }

        public string ShaderIdentifier => _pathFrag + _pathVert;

        ~Shader()
        {
            if (!_isDisposed)
                Scriptorium.Warn("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }
}
