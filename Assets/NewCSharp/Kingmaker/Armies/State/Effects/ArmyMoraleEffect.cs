using System;
using Kingmaker.Armies.State;
using Kingmaker.Crusade.GlobalMagic;

namespace Kingmaker.Armies.State.Effects
{
	[Serializable]
	public  class ArmyMoraleEffect : Ex.Kingmaker.Armies.State.Effects.ArmyMoraleEffect 
	{
		public ArmyMoraleEffect (Int32 moraleValue, TimeSpan endTime, ArmyEffectsManager manager, BlueprintGlobalMagicSpell globalMagicSpell) : base(moraleValue, endTime, manager, globalMagicSpell)
		{
		
		}
		ArmyMoraleEffect () : base()
		{
		
		}
	}
}
