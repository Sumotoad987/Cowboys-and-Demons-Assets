using System;
using Kingmaker.Globalmap.State;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Blueprints;

namespace Kingmaker.Globalmap.State
{
	[Serializable]
	public  class LocationTravelLogic : Ex.Kingmaker.Globalmap.State.LocationTravelLogic 
	{
		public LocationTravelLogic (GlobalMapArmyState state, BlueprintGlobalMapPoint targetPoint, Single hours, BlueprintActionList completeActions) : base(state, targetPoint, hours, completeActions)
		{
		
		}
		public LocationTravelLogic () : base()
		{
		
		}
	}
}
