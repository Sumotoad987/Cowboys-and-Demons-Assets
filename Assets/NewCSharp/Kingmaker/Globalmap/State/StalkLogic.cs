using System;
using Kingmaker.Globalmap.State;
using Kingmaker.Blueprints;

namespace Kingmaker.Globalmap.State
{
	[Serializable]
	public  class StalkLogic : Ex.Kingmaker.Globalmap.State.StalkLogic 
	{
		public StalkLogic (GlobalMapArmyState state, Single speed, BlueprintActionList completeActions, String targetArmy) : base(state, speed, completeActions, targetArmy)
		{
		
		}
		public StalkLogic () : base()
		{
		
		}
	}
}
