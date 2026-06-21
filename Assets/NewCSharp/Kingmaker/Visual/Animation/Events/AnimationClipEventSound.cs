using System;

namespace Kingmaker.Visual.Animation.Events
{
	[Serializable]
	public  class AnimationClipEventSound : Ex.Kingmaker.Visual.Animation.Events.AnimationClipEventSound 
	{
		public AnimationClipEventSound (Single time) : base(time)
		{
		
		}
		public AnimationClipEventSound (Single time, Boolean isLooped, String name, Single volume) : base(time, isLooped, name, volume)
		{
		
		}
	}
}
