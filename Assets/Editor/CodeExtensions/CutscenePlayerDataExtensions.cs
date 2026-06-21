using Kingmaker.AreaLogic.Cutscenes;

namespace Code.GameCore.Editor.CodeExtensions
{
	public static class CutscenePlayerDataExtensions
	{
		public static bool IsCommandFailed(this CutscenePlayerData playerData, CommandBase command)
		{
			//return playerData.FailedCommands.Contains(command);
			return false;
		}

		public static bool IsTrackFinished(this CutscenePlayerData playerData, Track track)
		{
			//return playerData.FinishedTracks.Contains(track);
			return true;
		}
	}
}