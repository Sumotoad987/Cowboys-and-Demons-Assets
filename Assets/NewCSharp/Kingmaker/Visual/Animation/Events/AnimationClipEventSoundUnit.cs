using System;
using Kingmaker.Visual.Animation.Events;

namespace Kingmaker.Visual.Animation.Events
{
	[Serializable]
	public  class AnimationClipEventSoundUnit : Ex.Kingmaker.Visual.Animation.Events.AnimationClipEventSoundUnit 
	{
		public AnimationClipEventSoundUnit (Single time) : base(time)
		{
		
		}
		public AnimationClipEventSoundUnit (Single time, SoundType type) : base(time, type)
		{
		
		}
	}
}
