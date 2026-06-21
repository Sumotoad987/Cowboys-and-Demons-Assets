using System;

namespace Kingmaker.Utility
{
	[Serializable]
	public  class ContextParameter : Ex.Kingmaker.Utility.ContextParameter 
	{
		public ContextParameter () : base()
		{
		
		}
		public ContextParameter (String name, String val) : base(name, val)
		{
		
		}
	}
}
