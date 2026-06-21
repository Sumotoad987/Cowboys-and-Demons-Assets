using Kingmaker.Blueprints.Area;
using System.Linq;

namespace Code.GameCore.Editor.CodeExtensions
{
	public static class BlueprintAreaPartExtension
	{
		public static void OpenInEditorWindow(this BlueprintAreaPart blueprintAreaPart)
		{
			//var scenes = blueprintAreaPart.GetStaticAndActiveDynamicScenes().Where(sceneReference => sceneReference.IsDefined);
			//var first = true;
			//foreach (var sceneReference in scenes)
			//{
			//	UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
			//		sceneReference.ScenePath,
			//		first
			//			? UnityEditor.SceneManagement.OpenSceneMode.Single
			//			: UnityEditor.SceneManagement.OpenSceneMode.Additive);
			//	first = false;
			//}
		}
	}
}