using System;
using Kingmaker.Crusade.GlobalMagic;

namespace Kingmaker.Crusade.GlobalMagic.SpellsManager
{
	[Serializable]
	public  class ActiveSpell : Ex.Kingmaker.Crusade.GlobalMagic.SpellsManager.ActiveSpell 
	{
		public ActiveSpell (BlueprintGlobalMagicSpell blueprint) : base(blueprint)
		{
		
		}
	}
}
