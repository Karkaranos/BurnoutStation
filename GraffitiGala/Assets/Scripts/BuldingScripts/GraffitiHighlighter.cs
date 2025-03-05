/*************************************************
Brandon Koederitz
3/5/2025
3/5/2025
Tracks the position of the most recently spawned piece of graffiti to highlight it.
***************************************************/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace GraffitiGala.City
{
    public class GraffitiHighlighter : MonoBehaviour
    {
        private static Transform trackedTransform;
        private static BuildingBehavior targetBuilding;
        private static event Action StartTrackingHighlight;
        private Coroutine trackRoutine;

        //private bool hasEnteredScreen;

        /// <summary>
        /// Sets the piece of graffiti this object is set to highlight.
        /// </summary>
        /// <param name="graffitiTransform">The transform to track.</param>
        public static void SetHighlightedGraffiti(Transform graffitiTransform, BuildingBehavior building)
        {
            Debug.Log("Tracked object set as " + graffitiTransform);
            trackedTransform = graffitiTransform;
            targetBuilding = building;
            StartTrackingHighlight?.Invoke();
        }

        /// <summary>
        /// Has the highlighters stop tracking when the building containing the graffiti goes off screen.
        /// </summary>
        public static void StopTracking(BuildingBehavior buildingThatReset)
        {
            if (buildingThatReset == targetBuilding)
            {
                trackedTransform = null;
                targetBuilding = null;
            }
        }

        /// <summary>
        /// Subscribe to the OnTrack event so that this object can be notified when a new object to track is given.
        /// </summary>
        private void Awake()
        {
            StartTrackingHighlight += StartTracking;
            gameObject.SetActive(false);
        }
        private void OnDestroy()
        {
            StartTrackingHighlight -= StartTracking;
        }

        /// <summary>
        /// Starts tracking the current target only if we arent already tracking.
        /// </summary>
        private void StartTracking()
        {
            Debug.Log("Starting to Track.");
            if (trackRoutine == null && trackedTransform != null)
            {
                gameObject.SetActive(true);
                trackRoutine = StartCoroutine(TrackingRoutine());
            }
        }

        /// <summary>
        /// Has this object continually snap to the position of the tracked object.
        /// </summary>
        /// <returns>Coroutine.</returns>
        private IEnumerator TrackingRoutine()
        {
            while (trackedTransform != null)
            {
                transform.position = trackedTransform.position;

                yield return null;
            }
            trackRoutine = null;
            gameObject.SetActive(false);
        }
    }
}