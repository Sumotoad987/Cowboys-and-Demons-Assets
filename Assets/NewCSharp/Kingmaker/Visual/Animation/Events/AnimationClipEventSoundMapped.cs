using System;
using Kingmaker.Visual.Sound;

namespace Kingmaker.Visual.Animation.Events
{
	[Serializable]
	public  class AnimationClipEventSoundMapped : Ex.Kingmaker.Visual.Animation.Events.AnimationClipEventSoundMapped 
	{
		public AnimationClipEventSoundMapped (Single time) : base(time)
		{
		
		}
		public AnimationClipEventSoundMapped (Single time, MappedAnimationEventType type) : base(time, type)
		{
		
		}
	}
}
