/*************************************************
Brandon Koederitz
3/5/2025
3/5/2025
Tracks the position of the most recently spawned piece of graffiti to highlight it.
***************************************************/
using System;
using System.Collections;
using UnityEngine;

namespace GraffitiGala.City
{
    public class GraffitiHighlighter : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField, Range(0f, 255f)] private float targetOpacity;
        [SerializeField] private float highlightTime;
        [SerializeField] private float fadeTime;
        private static SpriteRenderer trackedObject;
        //private static BuildingBehavior targetBuilding;
        private static event Action StartTrackingHighlight;
        private Coroutine trackRoutine;
        private Coroutine fadeRoutine;

        //private bool hasEnteredScreen;

        /// <summary>
        /// Sets the piece of graffiti this object is set to highlight.
        /// </summary>
        /// <param name="graffitiRenderer">The transform to track.</param>
        public static void SetHighlightedGraffiti(SpriteRenderer graffitiRenderer)
        {
            //Debug.Log("Tracked object set as " + graffitiRenderer);
            trackedObject = graffitiRenderer;
            //targetBuilding = building;
            StartTrackingHighlight?.Invoke();
        }

        ///// <summary>
        ///// Has the highlighters stop tracking when the building containing the graffiti goes off screen.
        ///// </summary>
        //public static void StopTracking(BuildingBehavior buildingThatReset)
        //{
        //    if (buildingThatReset == targetBuilding)
        //    {
        //        trackedTransform = null;
        //        targetBuilding = null;
        //    }
        //}

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
            //Debug.Log("Starting to Track.");
            if (trackRoutine == null && trackedObject != null)
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
            StartFadeRoutine(targetOpacity / 255f);

            float timer = highlightTime;
            while (timer > 0 && Camera.main.CheckObjectInCamera(trackedObject))
            {
                transform.position = trackedObject.transform.position;
                //transform.localScale = trackedTransform.localScale;

                timer -= Time.deltaTime;
                yield return null;
            }
            trackedObject = null;
            trackRoutine = null;
            StartFadeRoutine(0);
        }

        /// <summary>
        /// Ensures only 1 fade routine is happening at a time, and overwrites old fade routines with new ones.
        /// </summary>
        /// <param name="targetAlpha"></param>
        private void StartFadeRoutine(float targetAlpha)
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }
            fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
        }

        /// <summary>
        /// Fades this highlight in and out of view.
        /// </summary>
        /// <returns>Coroutine.</returns>
        private IEnumerator FadeRoutine(float targetAlpha)
        {
            if (targetAlpha > 0)
            {
                gameObject.SetActive(true);
            }
            float fadeTimer = 0;
            float startingAlpha = spriteRenderer.color.a;
            // Continually fades in the sprite until it is fully visible.
            while (fadeTimer < fadeTime)
            {
                float normalizedTime = fadeTimer / fadeTime;
                spriteRenderer.color = spriteRenderer.color.SetAlpha(Mathf.Lerp(startingAlpha, targetAlpha, 
                    normalizedTime));
                fadeTimer += Time.deltaTime;
                yield return null;
            }
            if (targetAlpha <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}