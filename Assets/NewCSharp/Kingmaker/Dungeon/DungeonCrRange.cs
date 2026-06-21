using System;
using Kingmaker.Dungeon;

namespace Kingmaker.Dungeon
{
	[Serializable]
	public  class DungeonCrRange : Ex.Kingmaker.Dungeon.DungeonCrRange 
	{
		public DungeonCrRange () : base()
		{
		
		}
		public DungeonCrRange (Int32 cr) : base(cr)
		{
		
		}
		public DungeonCrRange (DungeonCrRange crRange) : base(crRange)
		{
		
		}
		public DungeonCrRange (Int32 min, Int32 max) : base(min, max)
		{
		
		}
	}
}
