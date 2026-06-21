using System.Diagnostics;
using UnityEngine;

namespace Kingmaker.View
{
	public  class AreaEnterPoint : Ex.Kingmaker.View.AreaEnterPoint 
	{
		public AreaEnterPoint () : base()
		{
		
		}

        private void OnDrawGizmos()
        {
			Gizmos.DrawIcon(transform.position, "EntryPoint");
        }
    }
}
