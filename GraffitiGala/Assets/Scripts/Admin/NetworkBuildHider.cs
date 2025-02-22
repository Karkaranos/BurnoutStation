/*************************************************
Brandon Koederitz
2/21/2025
2/21/2025
Hides network elements, including if they are spawned, from certain builds.
FishNet
***************************************************/
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraffitiGala
{
    public class NetworkBuildHider : NetworkBehaviour
    {
        [SerializeField] private AllowedBuilds hiddenInBuilds;

        /// <summary>
        /// Hides this object if the BuildManager doesnt contain it's flag.  Seperate from the default BuildHider
        /// to hide spawned objects on certain builds.
        /// </summary>
        public override void OnStartClient()
        {
            if (BuildManager.CheckBuild(hiddenInBuilds))
            {
                gameObject.SetActive(false);
            }
        }
    }
}
