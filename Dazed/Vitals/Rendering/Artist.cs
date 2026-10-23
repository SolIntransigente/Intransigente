using Dazed.Vitals.Content;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using Dazed.Vitals.Rendering.RenderDebug;
using Dazed.Vitals.AssetReference;
using System.Numerics;

namespace Dazed.Vitals.Rendering
{
    public static class Artist
    {
        static int _fbo;
        static int _rbo;
        static int _frameBufferTexture;

        private static FullscreenRenderer _fullscreenRender = null!;

        [ExposedField("missing texture path")]
        private static AssetPath _missingTexturePath;
        public static Texture2D MissingTexture { get; }

        public static int Width { get => _width; internal set { _width = value; RefreshRenderingInfo(); } }
        private static int _width;
        public static int Height { get => _height; internal set { _height = value; RefreshRenderingInfo(); } }
        private static int _height;

        public static float NormalizedHeight => _normalizedHeight;
        private static float _normalizedHeight;

        public static float NormalizedWidth => _normalizedWidth;
        private static float _normalizedWidth;

        public static float AspectRatio { get => _aspectRatio; }
        private static float _aspectRatio;
        public static int DesiredCameraWidth
        {
            get => _desiredCameraWidth;
            set
            {
                _desiredCameraWidth = value;
                _desiredCameraAspectRatio = _desiredCameraWidth / (float)_desiredCameraHeight;
            }
        }

        private static int _desiredCameraWidth = 640;

        public static int DesiredCameraHeight
        {
            get => _desiredCameraHeight;
            set
            {
                _desiredCameraHeight = value;
                _desiredCameraAspectRatio = _desiredCameraWidth / (float)_desiredCameraHeight;
            }
        }
        private static int _desiredCameraHeight = 480;

        public static float DesiredCameraAspectRatio => _desiredCameraAspectRatio;
        private static float _desiredCameraAspectRatio = _desiredCameraWidth / (float)_desiredCameraHeight;

        private static float _viewportX, _viewportY;
        private static int _viewportWidth, _viewportHeight;

        public static event Action OnWidowResize = null!;

        private const short RenderElementsLimit = 4096;
        private static readonly Renderable?[] AllRender = new Renderable?[RenderElementsLimit];
        private static readonly RenderingPass[] RenderPasses = new RenderingPass[RenderElementsLimit];

        private static ViewportCamera? _secondaryCamera;

        private static readonly List<TemporalRenderable> TemporalRenderables = new();
        
        private static readonly List<short> GeometryFirst = new();
        private static readonly List<short> GeometrySecond = new();
        private static readonly List<short> Gui = new();
        private static bool _modifiedRendered = true;

        private const byte MaximumRenderedLights = 8;
        private const short MaximumLightInstances = 1024;

        private static readonly Light[] RenderedLights = new Light[MaximumRenderedLights];
        private static readonly LightCategory?[] LightCategories = new LightCategory?[MaximumRenderedLights];
        private static readonly Dictionary<LightCategory, byte> LightCounts = new(MaximumRenderedLights);

        private static readonly List<Light> LightInstances = new(MaximumLightInstances);
        private static readonly List<Material> LitMaterials = new(1024);

        private static byte CurrentPointLightCount, CurrentSpotLightCount;

        private static readonly LightParameters[] PrefabPointLightParameters = new LightParameters[MaximumRenderedLights];
        private static readonly LightParameters[] PrefabSpotLightParameters = new LightParameters[MaximumRenderedLights];
        private static readonly LightParameters PrefabDirectionalLightParameters;

        private const string PointLightStructName = "pointLights";
        private const string SpotLightStructName = "spotLights";
        private const string DirectionalLightStructName = "dirLight";

        private static int _currentMeshBinding;

        public static void ReloadScreenInfo()
        {
            SetUpFrameBuffer();
            _viewportHeight = Height;
            _viewportWidth = Width;
            if(Width * _desiredCameraWidth > Height * _desiredCameraHeight)
            {
                _viewportWidth = (int)(Height * _desiredCameraAspectRatio);
                _viewportX = (Width - _viewportWidth) / 2f;
                _viewportY = 0;
            }
            else if(Width * _desiredCameraWidth < Height * _desiredCameraHeight)
            {
                _viewportHeight = (int)(Width * _desiredCameraAspectRatio);
                _viewportY = (Height - _viewportHeight) / 2f;
                _viewportX = 0;
            }
            OnWidowResize?.Invoke();
        }
        private static void RefreshRenderingInfo()
        {
            _aspectRatio = _height / (float)_width;
            _normalizedHeight = 1 / (float)_height;
            _normalizedWidth = 1 / (float)_width;

            ReloadScreenInfo();
        }

        public static void SetResolution(int width, int height)
        {
            if (width <= 0 || height <= 0)
                return;
            _width = width;
            _height = height;
            RefreshRenderingInfo();
        }

        static Artist()
        {
            ExposedData.RetrieveAll(typeof(Artist), null);
            MissingTexture = Texture2D.GetTextureInstance(_missingTexturePath!.AssetRelativePath);

            var baseParams = Light.BaseParameters;
            string[] pointParams = [.. Light.BaseParameters, .. PointLight.Parameters];
            string[] spotParams = [.. Light.BaseParameters, .. SpotLight.Parameters];
            for (short p = 0; p < MaximumRenderedLights; p++)
            {
                var pointLightBase = $"{PointLightStructName}[{p}].";
                string[] fullParameters = GetParams(pointParams, pointLightBase);

                PrefabPointLightParameters[p] = new(fullParameters, pointParams);
            }
            for (byte s = 0; s < MaximumRenderedLights; s++)
            {
                var spotLightBase = $"{SpotLightStructName}[{s}].";
                var fullParameters = GetParams(spotParams, spotLightBase);

                PrefabSpotLightParameters[s] = new(fullParameters, spotParams);
            }
            var dirLightBase = $"{DirectionalLightStructName}.";
            string[] dirParams = [.. Light.BaseParameters, .. DirectionalLight.Parameters];
            var dirLightParams = GetParams(dirParams, dirLightBase);
            PrefabDirectionalLightParameters = new(dirLightParams, dirParams);
        }

        private static string[] GetParams(string[] allParams, string light)
        {
            var parameters = new string[allParams.Length];

            for(byte i = 0; i < allParams.Length; i++)
                parameters[i] = light + allParams[i];
            return parameters;
        }

        internal static void Initialize()
        {
            SetUpFrameBuffer();
            _fullscreenRender = new FullscreenRenderer();
        }

        private static void SetUpFrameBuffer()
        {
            if (_fbo == 0)
                _fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            if (_frameBufferTexture == 0)
                _frameBufferTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _frameBufferTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R3G3B2, DesiredCameraWidth, DesiredCameraHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, 0);
            GL.TextureParameter(_frameBufferTexture, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TextureParameter(_frameBufferTexture, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TextureParameter(_frameBufferTexture, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TextureParameter(_frameBufferTexture, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _frameBufferTexture, 0);

            if (_rbo == 0)
                _rbo = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, DesiredCameraWidth, DesiredCameraHeight);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _frameBufferTexture, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _rbo);

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                string message = "Frame buffer failed with status: " + status.ToString();
                Scriptorium.Error(message);
                throw new Exception(message);
            }
        }

        internal static void TriggerAllShadersReload()
        {
            if(DebugState.DebugLow)
                Scriptorium.Warn("Reloading all shader...");
            Shader.Reload();
        }

        internal static int AssignRendered(Renderable rendered)
        {
            if (_currentMeshBinding >= RenderElementsLimit)
                throw new IndexOutOfRangeException("Drawn mesh limit exceeded, either increase arbitrary limit or check for duplicated meshes");
            RenderPasses[_currentMeshBinding] = rendered.RenderPass;
            AllRender[_currentMeshBinding++] = rendered;
            
            _modifiedRendered = true;
            return _currentMeshBinding;
        }

        internal static void DisposeRendered(int bindPosition)
        {
            _modifiedRendered = true;
            AllRender[bindPosition] = null;
        }

        internal static void AssignSecondaryCamera(ViewportCamera viewportCamera)
        {
            if (_secondaryCamera != null)
            {
                if(DebugState.DebugMedium)
                    Scriptorium.Warn("A secondary camera is already assigned and yet another one is trying to replace it.");
                return;
            }
            _secondaryCamera = viewportCamera;
        }

        internal static void RemoveSecondaryCamera()
        {
            if (_secondaryCamera == null)
            {
                if(DebugState.DebugMedium)
                    Scriptorium.Warn("A path of execution is trying to remove a secondary camera when it is not yet assigned.");
                return;
            }
            _secondaryCamera = null;
        }

        internal static void DrawFrame(FrameEventArgs args)
        {
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);

            GL.Viewport(0, 0, _desiredCameraWidth, _desiredCameraHeight);
            RenderMeshed();
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            GL.Viewport((int)_viewportX, (int)_viewportY, _viewportWidth, _viewportHeight);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            RenderFramebuffer();
            GL.Viewport(0, 0, Width, Height);

            GuiRendering();
        }

        private static void RenderFramebuffer()
        {
            _fullscreenRender.Render(_frameBufferTexture);
        }

        private static void RenderMeshed()
        {
            ArrangePasses();
            ArrangeRenderedLights();
            CurrentPointLightCount = CurrentSpotLightCount = 0;
            for(int i = 0; i < LitMaterials.Count; i++)
            {
                for(byte L = 0; L < MaximumRenderedLights; L++)
                {
                    if (LightCategories[L] == null)
                        break;
                    LitMaterials[i].SetFloat("pointLightCount", LightCounts[LightCategory.Point]);
                    LitMaterials[i].SetFloat("spotLightCount", LightCounts[LightCategory.SpotLight]);
                    RenderedLights[L].SetValuesTo(LitMaterials[i], GetSentParameters(LightCategories[L]!.Value));
                }
            }
            if (ViewportCamera.MainCamera == null)
            {
                GL.ClearColor(Color.Black);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                return;
            }

            GL.ClearColor(Color.LightCyan);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            RenderingData data;
            data = new(ViewportCamera.MainCamera.Projection, ViewportCamera.MainCamera.View, ViewportCamera.MainCamera.Transform.Position, ViewportCamera.MainCamera.ViewFrustum);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            RenderPass(RenderingPass.geometryFirst, data);
            if (DebugState.DebugMedium)
            {
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);
                DrawTemporal(data);
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);
            }

            //Secondary Camera (if available)
            if (_secondaryCamera != null)
            {
                data = new(_secondaryCamera.Projection, _secondaryCamera.View, _secondaryCamera.Transform.Position, _secondaryCamera.ViewFrustum);
                GL.Clear(ClearBufferMask.DepthBufferBit);
                RenderPass(RenderingPass.geometrySecond, data);
            }
        }

        private static LightParameters GetSentParameters(LightCategory lightCategory)
        {
            return lightCategory switch
            {
                LightCategory.Point => PrefabPointLightParameters[CurrentPointLightCount++],
                LightCategory.SpotLight => PrefabSpotLightParameters[CurrentSpotLightCount++],
                LightCategory.Directional => PrefabDirectionalLightParameters
            };
        }

        private static void ArrangeRenderedLights()
        {
            RestoreCounts();
            byte currentCount = 0;
            short?[] indexes = new short?[MaximumRenderedLights];
            Half[] weights = new Half[MaximumRenderedLights];
            for(short i = 0; i < LightInstances.Count; i++)
            {
                if (LightInstances[i] == null)
                    continue;
                var weight = LightInstances[i].ProcessLightWeight(ViewportCamera.MainCamera);
                if (currentCount < MaximumRenderedLights)
                {
                    weights[currentCount] = weight;
                    indexes[currentCount++] = i;
                    continue;
                }
                for(short j = 0; j < indexes.Length; j++)
                {
                    if(weight > weights[j])
                    {
                        weights[j] = weight;
                        indexes[j] = i;
                    }
                }
            }
            for(byte c = 0; c < MaximumRenderedLights; c++)
            {
                LightCategories[c] = indexes[c].HasValue ? LightInstances[indexes[c].Value].Category : null;
                if (!indexes[c].HasValue)
                    continue;
                AddCount(LightCategories[c].Value);
                RenderedLights[c] = LightInstances[indexes[c].Value];
            }
        }

        private static void AddCount(LightCategory lightCategory) => LightCounts[lightCategory] = (byte)(LightCounts[lightCategory] + 1);

        private static void RestoreCounts()
        {
            LightCounts.Clear();
            LightCounts.Add(LightCategory.Point, 0);
            LightCounts.Add(LightCategory.SpotLight, 0);
            LightCounts.Add(LightCategory.Directional, 0);
            LightCounts.Add(LightCategory.Other, 0);
        }

        private static void DrawTemporal(in RenderingData data)
        {
            foreach (var t in TemporalRenderables)
                t.Render(data);
            TemporalRenderables.Clear();
        }

        private static void ArrangePasses()
        {
            if (!_modifiedRendered)
                return;
            _modifiedRendered = false;
            GeometryFirst.Clear();
            GeometrySecond.Clear();
            Gui.Clear();
            for(short i = 0; i < _currentMeshBinding; i++)
            {
                if (AllRender[i] == null)
                    continue;
                switch (RenderPasses[i])
                {
                    case RenderingPass.geometryFirst:
                        GeometryFirst.Add(i);
                        break;
                    case RenderingPass.geometrySecond:
                        GeometrySecond.Add(i);
                        break;
                    case RenderingPass.userInterface:
                        Gui.Add(i);
                        break;
                }
            }
        }

        private static void RenderPass(RenderingPass pass, in RenderingData? data)
        {
            List<short> indexes = null!;
            switch (pass)
            {
                case RenderingPass.geometryFirst:
                    indexes = GeometryFirst;
                    break;
                case RenderingPass.geometrySecond:
                    indexes = GeometrySecond;
                    break;
                case RenderingPass.userInterface:
                    indexes = Gui;
                    break;
            }
            foreach(int index in indexes)
            {
                Renderable? r = AllRender[index];
                if (r == null || !r.AllowRendering)
                    continue;
                r.Render(!pass.HasFlag(RenderingPass.userInterface) ? data!.Value : default);
            }
        }

        private static void GuiRendering() => RenderPass(RenderingPass.userInterface, null);

        public static void QueueTemporal(TemporalRenderable temp) => TemporalRenderables.Add(temp);

        internal static void RegisterNewLight(Light instance)
        {
            if (LightInstances.Count == MaximumLightInstances)
            {
                if (DebugState.DebugLow)
                    throw new IndexOutOfRangeException($"The maximum amount of light instances was reached <{MaximumLightInstances}>");
                return;
            }
            short index = (short)LightInstances.Count;
            LightInstances.Add(instance);
        }

        internal static void RemoveLightInstance(Light instance) => LightInstances.Remove(instance);

        internal static void AddLitMaterial(Material material) => LitMaterials.Add(material);
    }
}
