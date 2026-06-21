using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Globalmap.Blueprints;

namespace Kingmaker.QA.Clockwork
{
[TypeIdAttribute("c2efcfd2e96ad6d42833ef1a72f67183")]
	public  class TravelGlobalMapCommand : Ex.Kingmaker.QA.Clockwork.TravelGlobalMapCommand 
	{
		public TravelGlobalMapCommand () : base()
		{
		
		}
		public TravelGlobalMapCommand (BlueprintGlobalMapPoint targetLocation) : base(targetLocation)
		{
		
		}
	}
}
