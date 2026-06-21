using System;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Customization;

namespace Kingmaker.UnitLogic.Customization
{
	[Serializable]
	public  class UnitCustomizationVariation : Ex.Kingmaker.UnitLogic.Customization.UnitCustomizationVariation 
	{
		UnitCustomizationVariation () : base()
		{
		
		}
		public UnitCustomizationVariation (BlueprintRace race, Gender gender) : base(race, gender)
		{
		
		}
		public UnitCustomizationVariation (UnitCustomizationVariation other) : base(other)
		{
		
		}
	}
}
