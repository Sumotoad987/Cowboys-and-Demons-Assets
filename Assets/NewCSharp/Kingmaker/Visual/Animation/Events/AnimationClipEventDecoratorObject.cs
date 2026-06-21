using System;
using Kingmaker.View.Animation;

namespace Kingmaker.Visual.Animation.Events
{
	public  class AnimationClipEventDecoratorObject : Ex.Kingmaker.Visual.Animation.Events.AnimationClipEventDecoratorObject 
	{
		public AnimationClipEventDecoratorObject (Single time) : base(time)
		{
		
		}
		public AnimationClipEventDecoratorObject (Single time, UnitAnimationDecoratorObject decoratorObject) : base(time, decoratorObject)
		{
		
		}
	}
}
