using System;

namespace Kingmaker.Visual.Animation.Events
{
	[Serializable]
	public  class AnimationClipEventSoundSurface : Ex.Kingmaker.Visual.Animation.Events.AnimationClipEventSoundSurface 
	{
		public AnimationClipEventSoundSurface (Single time) : base(time)
		{
		
		}
		public AnimationClipEventSoundSurface (Single time, String name) : base(time, name)
		{
		
		}
	}
}
