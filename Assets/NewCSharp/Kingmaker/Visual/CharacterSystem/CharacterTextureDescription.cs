using System;
using Kingmaker.Visual.CharacterSystem;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem
{
	[Serializable]
	public  class CharacterTextureDescription : Ex.Kingmaker.Visual.CharacterSystem.CharacterTextureDescription 
	{
		public CharacterTextureDescription () : base()
		{
		
		}
		public CharacterTextureDescription (CharacterTextureChannel channel, Texture2D mainTexture) : base(channel, mainTexture)
		{
		
		}
	}
}
