namespace Owlcat.Runtime.Visual.RenderPipeline.PostProcess
{
	public interface IPostProcessComponent
	{
		bool IsActive();

		bool IsTileCompatible();
	}
}
