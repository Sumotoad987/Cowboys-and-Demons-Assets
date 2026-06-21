using System;
using Kingmaker.EntitySystem.Persistence.JsonUtility;

namespace Kingmaker.Kingdom
{
	[Serializable]
	public  class KingdomStats : Ex.Kingmaker.Kingdom.KingdomStats 
	{
		public KingdomStats () : base()
		{
		
		}
		KingdomStats (JsonConstructorMark _) : base(_)
		{
		
		}
	}
}
