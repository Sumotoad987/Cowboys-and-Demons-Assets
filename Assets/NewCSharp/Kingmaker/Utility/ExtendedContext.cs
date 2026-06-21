using System;
using System.Collections.Generic;

namespace Kingmaker.Utility
{
	[Serializable]
	public  class ExtendedContext : Ex.Kingmaker.Utility.ExtendedContext 
	{
		public ExtendedContext (List<String> historyLog) : base(historyLog)
		{
		
		}
	}
}
