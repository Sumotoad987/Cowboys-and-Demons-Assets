using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Kingmaker.Visual.Particles
{
	public  class FxLocator : Ex.Kingmaker.Visual.Particles.FxLocator 
	{
		public FxLocator () : base()
		{

        }

        new void OnDrawGizmos()
        {
            if (particleMap && !particleMap.ShowGizmos)
            {
                return;
            }
            Gizmos.DrawIcon(transform.position, "locator2.png");
        }
    }
}
