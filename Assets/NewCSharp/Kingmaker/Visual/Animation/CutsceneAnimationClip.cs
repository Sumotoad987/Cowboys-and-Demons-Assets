using System;
using UnityEngine;

namespace Kingmaker.Visual.Animation
{
	[Serializable]
	public  class CutsceneAnimationClip : Ex.Kingmaker.Visual.Animation.CutsceneAnimationClip 
	{
		public CutsceneAnimationClip (AnimationClip clip, Single transitionIn, Single transitionOut, Boolean isConsisten, Boolean isEndless) : base(clip, transitionIn, transitionOut, isConsisten, isEndless)
		{
		
		}
	}
}
