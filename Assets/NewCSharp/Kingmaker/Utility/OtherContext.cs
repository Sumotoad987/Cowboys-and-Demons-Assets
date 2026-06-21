using System;
using System.Collections.Generic;
using Kingmaker.Utility;

namespace Kingmaker.Utility
{
	[Serializable]
	public  class OtherContext : Ex.Kingmaker.Utility.OtherContext 
	{
		public OtherContext (List<BugContext> contextVariants, BugContext currentContext) : base(contextVariants, currentContext)
		{
		
		}
	}
}
