using System;
using System.Collections.Generic;
using Kingmaker.Utility;

namespace Kingmaker.Utility
{
	[Serializable]
	public  class PartyContext : Ex.Kingmaker.Utility.PartyContext 
	{
		public PartyContext (List<ReportParameterHelper> itemsHistory, List<ReportParameterHelper> spellsHistory) : base(itemsHistory, spellsHistory)
		{
		
		}
	}
}
