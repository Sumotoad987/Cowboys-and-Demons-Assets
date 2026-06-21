using Kingmaker.Blueprints.JsonSystem;
using UnityEngine;

namespace Kingmaker.QA.Clockwork
{
[TypeIdAttribute("0a574d5d87976144dbadf8f10142a798")]
	public  class MoveCommand : Ex.Kingmaker.QA.Clockwork.MoveCommand 
	{
		public MoveCommand () : base()
		{
		
		}
		public MoveCommand (Vector3 pointToMove) : base(pointToMove)
		{
		
		}
	}
}
