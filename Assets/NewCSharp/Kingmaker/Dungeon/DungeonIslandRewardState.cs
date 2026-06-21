using System;
using Kingmaker.Dungeon.Blueprints;

namespace Kingmaker.Dungeon
{
	[Serializable]
	public  class DungeonIslandRewardState : Ex.Kingmaker.Dungeon.DungeonIslandRewardState 
	{
		DungeonIslandRewardState () : base()
		{
		
		}
		public DungeonIslandRewardState (BlueprintDungeonIslandReward reward) : base(reward)
		{
		
		}
	}
}
