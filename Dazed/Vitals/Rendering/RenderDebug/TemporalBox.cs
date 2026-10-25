using OpenTK.Graphics.OpenGL;
using System.Numerics;
namespace Dazed.Vitals.Rendering.RenderDebug;

internal class TemporalBox : TemporalRenderable
{
	private static readonly float[] Vertices =
	[
		-1f,  1f, -1f,
		-1f, -1f, -1f,
		1f, -1f, -1f,
		1f,  1f, -1f
	];

	private static readonly uint[] Indices =
	[
		0, 1, 2,
		2, 3, 0
	];
	private static readonly int Vao;
	
	public Matrix4x4 World { get; set; }
	public Color Color { get; set; }
	public bool IgnoreViewProjection { get; set; }

	static TemporalBox()
	{
		var vertexBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
		GL.BufferStorage(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferStorageFlags.None);
		//GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StaticDraw);

		var elementBufferObject = GL.GenBuffer();
		Vao = GL.GenVertexArray();
		GL.BindVertexArray(Vao);
		int positionLocation = DebugShader.GetAttribLocation("aPos");
		GL.EnableVertexAttribArray(positionLocation);
		GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
		
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
		GL.BufferStorage(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices, BufferStorageFlags.None);
		//GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices, BufferUsageHint.StaticDraw);

		GL.BindVertexArray(0);
	}
	
	public override void Render(in RenderingData data)
	{
		GL.BindVertexArray(Vao);
		DebugShader.Use();
		SetUpShaderData(data);
		GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
		GL.BindVertexArray(0);
	}

	private void SetUpShaderData(in RenderingData data)
	{
		DebugShader.SetMatrix4("model", World);
		DebugShader.SetMatrix4("view", !IgnoreViewProjection ? data.ViewMatrix : Matrix4x4.Identity);
		DebugShader.SetMatrix4("projection", !IgnoreViewProjection ? data.ProjectionMatrix :  Matrix4x4.Identity);
		DebugShader.SetVector3("objectColor", Color.ToVector3());
	}
}