using System;
using System.Collections.Generic;
using Kingmaker.Utility;

namespace Kingmaker.Utility
{
	[Serializable]
	public  class ContextRow : Ex.Kingmaker.Utility.ContextRow 
	{
		public ContextRow () : base()
		{
		
		}
		public ContextRow (IEnumerable<String> row) : base(row)
		{
		
		}
		public ContextRow (IEnumerable<ContextParameter> parameters) : base(parameters)
		{
		
		}
	}
}
