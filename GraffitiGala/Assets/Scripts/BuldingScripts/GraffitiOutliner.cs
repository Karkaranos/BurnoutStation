/*************************************************
Brandon Koederitz
3/5/2025
3/5/2025
Adds an outline to newly spawned graffiti objects.
IO
***************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraffitiGala.City
{
    public class GraffitiOutliner : MonoBehaviour
    {
        private SpriteRenderer sRend;
        private BuildingBehavior buildingListener;
        private Material storedMaterial;

        /// <summary>
        /// Initializes the outline on this graffiti.
        /// </summary>
        /// <param name="buildingListener">The builidng that this graffit is on.  Used to listen for when it gets reset.</param>
        /// <param name="rendererReference">A reference to the sprite renderer on this object.</param>
        public void Initialize(Material outlineMaterial, BuildingBehavior buildingListener, SpriteRenderer rendererReference = null)
        {
            BuildingScroller.OnBuildingToBack += RemoveOutline;
            this.buildingListener = buildingListener;
            sRend = rendererReference != null ? rendererReference : GetComponent<SpriteRenderer>();
            storedMaterial = sRend.material;
            sRend.material = outlineMaterial;
        }

        /// <summary>
        /// Removes the outline from this graffiti as well as this component when the building that this graffiti is on
        /// gets reset to the back and therefore has passed the screen once.
        /// </summary>
        /// <param name="buildingToCheck">The building that was broadcast as having been moved to the back.</param>
        private void RemoveOutline(BuildingBehavior buildingToCheck)
        {
            if (buildingToCheck == buildingListener)
            {
                sRend.material = storedMaterial;
                // Remove this component when the graffiti resets to clear up memory because this component has no more use.
                Destroy(this);
            }
        }

        /// <summary>
        /// Remove the event subscription anyways in case this object gets destroyed before RemoveOutline is called.
        /// </summary>
        private void OnDestroy()
        {
            BuildingScroller.OnBuildingToBack -= RemoveOutline;
        }
    }
}