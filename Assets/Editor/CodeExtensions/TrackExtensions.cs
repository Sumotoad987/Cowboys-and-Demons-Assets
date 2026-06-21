using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;

namespace Code.GameCore.Editor.CodeExtensions
{
	public static class TrackExtensions
	{
		public static Gate EditorEndGate(this Gate gate)
		{
			#if OWLCAT_MODS
			return gate?.ReloadFromInstanceID();
			#else
			return gate;
			#endif
		}
	}
}