using Dazed.Vitals;
using Dazed.Vitals.Rendering;
using DazedGUITools.Windows.LevelEditor.LevelEditorTools;
using DazedGUITools.Windows.LevelEditor.Representations;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DazedGUITools.Windows.LevelEditor
{
    internal static class LevelEditorContext
    {
        private const short HeightOffset = 45/*pixels*/;
        private const short WidthOffset = 15/*pixels*/;
        private static int s_fbo, s_rbo;
        internal static int FrameBufferTexture { get; private set; }
        public static short Width => s_width;
        public static short Height => s_height;
        public static float AspectRatio { get; internal set; }
        private static short s_width = 1, s_height = 1;

        private static ILevelEditorTool s_currentTool = null!;
        public static GeometryRepresentation? SelectedGeometry { get; private set; }
        public static int SelectedVertexIndex { get; private set; }
        private static int s_maximumVertexIndex;

        //TOTAL geometry
        // private static readonly GeometryRepresentation[] s_geometry = new GeometryRepresentation[4096];
        private static readonly List<SectionRepresentation> s_sections = new ();
        private static readonly Dictionary<SectionRepresentation, List<GeometryRepresentation>> s_sectionGeometry = new ();
        public static int GeometryCount { get; private set; }

        public static List<SectionRepresentation> AllSections => s_sections;

        public static SectionRepresentation CurrentlySelectedSection => s_currentSection;

        private readonly static VertexHighlight s_vertexHighlight;
        private static SectionRepresentation s_currentSection;

        static LevelEditorContext()
        {
            s_currentSection = null!;
            CreateAndAddNewSection();
            GeometryCount = 0;
            AspectRatio = 1;
            s_vertexHighlight = new();
        }

        internal static void CreateAndAddNewSection()
        {
            var section = new SectionRepresentation((ushort)s_sections.Count);
            s_sections.Add(section);
            if (s_currentSection == null)
                s_currentSection = section;
            s_sectionGeometry.Add(section, new());
        }

        public static void SelectSection(ushort index)
        {
            s_currentSection = s_sections[index];
        }

        internal static void Initialize(short width, short height)
        {
            SetTool(new TranslateLETool());
            LevelEditorCamera.Initialize();
            TryUpdateSize(width, height);
        }

        private static void SetUpFrameBuffer()
        {
            if (s_fbo == 0)
                s_fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, s_fbo);
            if (FrameBufferTexture == 0)
                FrameBufferTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, FrameBufferTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, s_width, s_height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, 0);
            GL.TextureParameter(FrameBufferTexture, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TextureParameter(FrameBufferTexture, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TextureParameter(FrameBufferTexture, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TextureParameter(FrameBufferTexture, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, FrameBufferTexture, 0);

            if (s_rbo == 0)
                s_rbo = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, s_rbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, s_width, s_height);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, FrameBufferTexture, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, s_rbo);

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                string message = "Frame buffer failed with status: " + status.ToString();
                Scriptorium.Error(message);
            }
            LevelEditorCamera.UpdateProjection();
        }

        internal static void TryUpdateSize(short x, short y)
        {
            if (x <= 0) 
                return;
            if (y <= 0)
                return;
            if(x == s_width && y == s_height) return;
            s_width = (short)MathF.Max(1, x - WidthOffset);
            s_height = (short)MathF.Max(1, y - HeightOffset);
            AspectRatio = s_width / (float)s_height;
            SetUpFrameBuffer();
        }

        internal static void Draw()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, s_fbo);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Viewport(0, 0, s_width, s_height);
            //---------------------------------
            s_currentTool?.Operate();
            for(int i = 0; i < s_sections.Count; i++)
            {
                if (!s_sections[i].Visible)
                    continue;
                foreach (var geom in s_sectionGeometry[s_sections[i]])
                    geom?.Draw();
            }
            //---------------------------------
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            RenderSelectedVertex();
            GL.Viewport(0, 0, MainWindow.Width, MainWindow.Height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private static void RenderSelectedVertex()
        {
            if(SelectedGeometry == null)
                return;
            s_vertexHighlight.DrawAt(SelectedGeometry.VertexAt(SelectedVertexIndex).Position);
        }

        internal static void AddSelectedVertexPosition(Vector3 addedPos) => SelectedGeometry?.AddPosition(SelectedVertexIndex, addedPos);

        internal static void SetTool(ILevelEditorTool tool)
        {
            s_currentTool?.Terminate();
            s_currentTool = tool;
            tool.Initialize();
        }

        internal static void SelectGeometry(GeometryRepresentation geom)
        {
            SelectedGeometry = geom;
            SelectedVertexIndex = 0;
            s_maximumVertexIndex = SelectedGeometry.VertexCount - 1;
        }

        internal static void ChangeVertex(bool add = true)
        {
            if (SelectedGeometry == null)
                return;
            SelectedVertexIndex += add ? 1 : -1;
            if(SelectedVertexIndex > s_maximumVertexIndex)
                SelectedVertexIndex = 0;
            if(SelectedVertexIndex < 0)
                SelectedVertexIndex = s_maximumVertexIndex;
        }

        internal static void DeleteCurrentQuad()
        {
            if (SelectedGeometry == null)
                return;
            if (s_sectionGeometry.TryGetValue(SelectedGeometry.ParentedSection, out var val))
                val.Remove(SelectedGeometry);
            else
                throw new NullReferenceException(nameof(s_sectionGeometry) + " | " + nameof(SelectedGeometry));
            s_sectionGeometry[SelectedGeometry.ParentedSection] = val;
            GeometryCount--;
        }

        internal static void AddNewQuad(Vector3 min, Vector3 max)
        {
            var quad = GeometryRepresentation.QuadGeometry(min, max);
            if (s_sectionGeometry.TryGetValue(CurrentlySelectedSection, out var geoL))
                geoL.Add(quad);
            else
                geoL = [quad];
            s_sectionGeometry[CurrentlySelectedSection] = geoL;
            SelectGeometry(quad);
            GeometryCount++;
        }

        internal static void AddGeometryPosition(Vector3 addedPos)
        {
            if (SelectedGeometry == null)
                return;
            SelectedGeometry.AddPosition(addedPos);
        }

        internal static VertexData? GetCurrentVertex()
        {
            if (SelectedGeometry == null)
                return null;
            return SelectedGeometry.VertexAt(SelectedVertexIndex);
        }
    }
}
