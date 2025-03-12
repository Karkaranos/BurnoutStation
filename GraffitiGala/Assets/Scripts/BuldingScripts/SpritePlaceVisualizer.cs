/*************************************************
Brandon Koederitz
3/3/2025
3/6/2025
Creates a visually large sprite on screen and scales it down to place it on it's renderer.  Purely a visual script.
***************************************************/
using System.Collections;
using System.Threading;
using UnityEngine;

namespace GraffitiGala.City
{
    public class SpritePlaceVisualizer : MonoBehaviour
    {
        private const string PARENT_TAG = "SpriteVisualizerParent";
        private static Transform parentTransform;

        /// <summary>
        /// Plays a visual where a large sprite is created then it shrinks down to match a given sprite renderer.
        /// </summary>
        /// <param name="renderer">The sprite renderer to copy.</param>
        /// <param name="settings">The settings to use for this sprite tween.</param>
        /// <param name="targetCam">The camera that this sprite will be displayed on.</param>
        public static void PlaceSpriteVisual(SpriteRenderer renderer, SpriteVisualizerSettings settings)
        {
            if (parentTransform == null)
            {
                parentTransform = GameObject.FindGameObjectWithTag(PARENT_TAG).transform;
            }

            GameObject go = new GameObject();
            go.transform.SetParent(parentTransform, false);
            // Set up the sprite renderer object.
            SpriteRenderer sRend = go.AddComponent<SpriteRenderer>();
            sRend.sprite = renderer.sprite;
            sRend.sortingLayerName = settings.SortingLayerName;
            sRend.sortingOrder = settings.OrderInLayer;
            // Sets the color of the temporary sprite renderer to match the target renderer, except it is transparent.
            // The sprite will fade in as it appears on screen.
            sRend.color = renderer.color;
            sRend.color = sRend.color.SetAlpha(0);
            SpritePlaceVisualizer visualizer = go.AddComponent<SpritePlaceVisualizer>();
            go.transform.localScale = new Vector3(settings.SpriteMagnification, settings.SpriteMagnification, 1);
            go.transform.localPosition = Vector3.zero;
            // This makes the assumption that the camera we are using is the main camera.
            visualizer.Tween(renderer, sRend, settings, Camera.main);
        }

        /// <summary>
        /// Tweens this object's transform to match the target transform.
        /// </summary>
        /// <param name="targetRenderer">The sprite renderer to copy.</param>
        /// <param name="thisRenderer">The sprite renderer on this object.</param>
        /// <param name="settings">The settings to use for this sprite tween.</param>
        /// <param name="targetCam">The camera that this sprite will be displayed on.</param>
        private void Tween(SpriteRenderer targetRenderer, SpriteRenderer thisRenderer, 
            SpriteVisualizerSettings settings, Camera targetCam)
        {
            StartCoroutine(TweenCoroutine(targetRenderer, thisRenderer, settings, targetCam));
        }

        /// <summary>
        /// Tweens this object to match the target transform.
        /// </summary>
        /// <param name="targetRenderer">The sprite renderer to copy.</param>
        /// <param name="thisRenderer">The sprite renderer on this object.</param>
        /// <param name="settings">The settings to use for this sprite tween.</param>
        /// <param name="targetCam">The camera that this sprite will be displayed on.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator TweenCoroutine(SpriteRenderer targetRenderer, SpriteRenderer thisRenderer,
            SpriteVisualizerSettings settings, Camera targetCam)
        {
            Transform targetTransform = targetRenderer.transform;
            // Hides the target sprite.
            targetRenderer.enabled = false;

            float fadeTimer = 0;
            // Continually fades in the sprite until it is fully visible.
            while (fadeTimer < settings.FadeInDuration)
            {
                float normalizedTime = fadeTimer / settings.FadeInDuration;
                thisRenderer.color = thisRenderer.color.SetAlpha(normalizedTime);
                fadeTimer += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(settings.InitialDelay);

            // Waits until the sprite renderer is actually inside the camera's view.
            while (!targetCam.CheckObjectInCamera(targetRenderer))
            {
                yield return null;
            }
            // Snapshot all of our transform values when we start the tween so that we have a starting point of reference
            // to LERP between.
            Vector3 snapshotPos = transform.position;
            Vector3 snapshotScale = transform.localScale;
            Vector3 snapshotRot = transform.eulerAngles;

            float timer = 0;

            while (timer < settings.TweenTime)
            {
                float normalizedTime = timer / settings.TweenTime;
                // uses the settings' animation curve to find where along a LERP path between the current transform and
                // the target transform we should be at.
                float lerpVal = settings.TweenCurve.Evaluate(normalizedTime);

                transform.position = Vector3.Lerp(snapshotPos, targetTransform.position, lerpVal);
                transform.localScale = Vector3.Lerp(snapshotScale, targetTransform.localScale, lerpVal);
                transform.eulerAngles = Vector3.Lerp(snapshotRot, targetTransform.eulerAngles, lerpVal);

                timer += Time.deltaTime;
                yield return null;
            }
            // Shows the target sprite.
            targetRenderer.enabled = true;
            Destroy(gameObject);
        }
    }
}