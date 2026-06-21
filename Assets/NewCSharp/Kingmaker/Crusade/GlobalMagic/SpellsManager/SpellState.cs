using System;
using Kingmaker.Crusade.GlobalMagic;

namespace Kingmaker.Crusade.GlobalMagic.SpellsManager
{
	[Serializable]
	public  class SpellState : Ex.Kingmaker.Crusade.GlobalMagic.SpellsManager.SpellState 
	{
		public SpellState (BlueprintGlobalMagicSpell blueprint) : base(blueprint)
		{
		
		}
		SpellState () : base()
		{
		
		}
	}
}
