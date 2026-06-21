using System;

namespace Kingmaker.Visual.Animation.Events
{
	[Serializable]
	public  class AnimationClipEventSoundWithPrefix : Ex.Kingmaker.Visual.Animation.Events.AnimationClipEventSoundWithPrefix 
	{
		public AnimationClipEventSoundWithPrefix (Single time) : base(time)
		{
		
		}
		public AnimationClipEventSoundWithPrefix (Single time, Boolean isLooped, String name, Single volume) : base(time, isLooped, name, volume)
		{
		
		}
	}
}
