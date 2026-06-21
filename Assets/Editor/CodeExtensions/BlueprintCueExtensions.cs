using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Kingmaker.DialogSystem.Blueprints;
using UnityEditor;

namespace Code.GameCore.Editor.CodeExtensions
{
	public static class BlueprintCueExtensions
	{
		#if UNITY_EDITOR && EDITOR_FIELDS
		public static void UpdateSpeaker(BlueprintCue cue, string defaultSpeakerName = "", string defaultSpeakerGender = "")
		{
			if (cue.Speaker.Blueprint == null && !cue.Speaker.NoSpeaker &&
			    (defaultSpeakerName == "" || defaultSpeakerGender == ""))
				return;
            
			string speakerName = cue.Speaker.Blueprint != null ? cue.Speaker.Blueprint.AssetName : defaultSpeakerName;
			string speakerGender = cue.Speaker.Blueprint != null ? cue.Speaker.Blueprint.Gender.ToString() : defaultSpeakerGender;
			var prop = new SerializedObject(BlueprintEditorWrapper.Wrap(cue)).FindProperty("Blueprint.Text");
			cue.Text.UpdateSpeaker(prop, speakerName, speakerGender);
		}
	#endif
	}
}