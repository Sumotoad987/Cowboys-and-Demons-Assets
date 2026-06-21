using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem
{
	public abstract class WeatherCustomEntitySettings : ScriptableObject
	{
		public virtual bool IsSoundSetting => false;

		public abstract IWeatherEntityController GetController(Transform root);
	}
}
