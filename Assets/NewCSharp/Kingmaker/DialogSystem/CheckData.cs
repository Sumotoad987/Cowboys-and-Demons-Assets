using System;
using Kingmaker.EntitySystem.Stats;

namespace Kingmaker.DialogSystem
{
	[Serializable]
	public  class CheckData : Ex.Kingmaker.DialogSystem.CheckData 
	{
		public CheckData (StatType type, Int32 dc) : base(type, dc)
		{
		
		}
	}
}
