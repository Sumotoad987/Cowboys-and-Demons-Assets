using System;
using UnityEngine;
using System.Collections.Generic;
using Kingmaker.Visual.Animation.Events;

namespace Kingmaker.Visual.Animation
{
	[Serializable]
	[CreateAssetMenu(fileName = "AnimationClipWithEvents", menuName = "Animation Manager/Animation Clip with Events")]
	public  class AnimationClipWrapper : Ex.Kingmaker.Visual.Animation.AnimationClipWrapper 
	{
		public AnimationClipWrapper (AnimationClip animationClip, IEnumerable<AnimationClipEventTrack> animationSoundTracks) : base(animationClip, animationSoundTracks)
		{
		
		}
	}
}
