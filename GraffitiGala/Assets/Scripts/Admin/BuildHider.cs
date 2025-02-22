/*************************************************
Brandon Koederitz
2/21/2025
2/21/2025
Hides certain object on a given client based on the BuildType of this build's BuildManager.
***************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraffitiGala
{
    public class BuildHider : MonoBehaviour
    {
        [SerializeField] private AllowedBuilds hiddenInBuilds;

        /// <summary>
        /// Hides this object if the BuildManager doesnt contain it's flag.
        /// </summary>
        private void Awake()
        {
            if (BuildManager.CheckBuild(hiddenInBuilds))
            {
                //Debug.Log("Hiding object " + name);
                gameObject.SetActive(false);
            }
        }
    }

}