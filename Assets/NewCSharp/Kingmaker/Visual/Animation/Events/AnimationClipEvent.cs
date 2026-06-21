using System;

namespace Kingmaker.Visual.Animation.Events
{
	[Serializable]
	public  class AnimationClipEvent : Ex.Kingmaker.Visual.Animation.Events.AnimationClipEvent 
	{
		public AnimationClipEvent (Single time) : base(time)
		{
		
		}
		public AnimationClipEvent (Single time, Boolean isLooped) : base(time, isLooped)
		{
		
		}
	}
}
