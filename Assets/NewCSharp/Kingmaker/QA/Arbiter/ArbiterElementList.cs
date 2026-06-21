using System;

namespace Kingmaker.QA.Arbiter
{
	[Serializable]
	public  class ArbiterElementList : Ex.Kingmaker.QA.Arbiter.ArbiterElementList 
	{
		public ArbiterElementList () : base()
		{
		
		}
		public ArbiterElementList (Type listElementType) : base(listElementType)
		{
		
		}
	}
}
