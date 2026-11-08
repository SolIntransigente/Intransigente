namespace Dazed.Vitals.Rendering.RenderDebug;

/// <summary>
/// Intended to be used for debug purposes, like a bounding box or a bounding sphere
/// </summary>
public abstract class TemporalRenderable : Renderable
{
	protected static readonly Shader DebugShader;
	protected override bool CustomAssignment => true;

	static TemporalRenderable()
	{
		DebugShader = Shader.GetNewShaderInstance("simple.vert", "debug.frag");
	}

	public TemporalRenderable()
	{
		Artist.QueueTemporal(this);
	}
	
	public override RenderingPass RenderPass => RenderingPass.geometrySecond;
	public override bool AllowRendering => true;
}