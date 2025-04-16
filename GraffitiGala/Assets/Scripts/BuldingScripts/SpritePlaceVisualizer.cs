/*************************************************
Brandon Koederitz
3/3/2025
3/6/2025
Creates a visually large sprite on screen and scales it down to place it on it's renderer.  Purely a visual script.
***************************************************/
using System.Collections;
using UnityEditor.UIElements;
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

            // Creates a visualizer with a given sprite and color.
            SpriteRenderer CreateVisualRenderer(Sprite visualizerSprite, Transform parent, Color color, int layerOffset = 0, float scaleModifier = 1f)
            {
                GameObject go = new GameObject();
                go.transform.SetParent(parent, false);
                // Set up the sprite renderer object.
                SpriteRenderer sRend = go.AddComponent<SpriteRenderer>();
                sRend.sprite = visualizerSprite;
                sRend.sortingLayerName = settings.SortingLayerName;
                sRend.sortingOrder = settings.OrderInLayer + layerOffset;
                // Sets the color of the temporary sprite renderer to match the target renderer, except it is transparent.
                // The sprite will fade in as it appears on screen.
                sRend.color = color;
                sRend.color = sRend.color.SetAlpha(0);

                go.transform.localScale = new Vector3(settings.SpriteMagnification, settings.SpriteMagnification, 1)
    * scaleModifier;
                go.transform.localPosition = Vector3.zero;
                return sRend;
            }

            // Creates the sprite renderer object for the graffiti itself.
            SpriteRenderer graffitiRend = CreateVisualRenderer(renderer.sprite, parentTransform, renderer.color);
            // Creates the sprite renderer object for the background sprite.
            SpriteRenderer backgroundRend = CreateVisualRenderer(settings.BackgroundSprite, graffitiRend.transform, Color.white, -1, settings.BackgroundScaleMoifier);

            SpritePlaceVisualizer visualizer = graffitiRend.gameObject.AddComponent<SpritePlaceVisualizer>();
            // This makes the assumption that the camera we are using is the main camera.
            visualizer.Tween(renderer, new SpriteRenderer[] { graffitiRend, backgroundRend }, settings, Camera.main);
        }


        /// <summary>
        /// Tweens this object's transform to match the target transform.
        /// </summary>
        /// <param name="targetRenderer">The sprite renderer to copy.</param>
        /// <param name="UpdateRenderers">The sprite renderer on this object.</param>
        /// <param name="settings">The settings to use for this sprite tween.</param>
        /// <param name="targetCam">The camera that this sprite will be displayed on.</param>
        /// <param name="playSFX">
        /// Whether this visualizer should play sound effects when it gets placed.  here to prevent double sounds from
        /// happening when a background is placed.
        /// </param>
        private void Tween(SpriteRenderer targetRenderer, SpriteRenderer[] UpdateRenderers, 
            SpriteVisualizerSettings settings, Camera targetCam)
        {
            StartCoroutine(TweenCoroutine(targetRenderer, UpdateRenderers, settings, targetCam));
        }

        /// <summary>
        /// Tweens this object to match the target transform.
        /// </summary>
        /// <param name="targetRenderer">The sprite renderer to copy.</param>
        /// <param name="updateRenderers">The sprite renderers on this object.</param>
        /// <param name="settings">The settings to use for this sprite tween.</param>
        /// <param name="targetCam">The camera that this sprite will be displayed on.</param>
        /// <param name="playSFX">
        /// Whether this visualizer should play sound effects when it gets placed.  here to prevent double sounds from
        /// happening when a background is placed.
        /// </param>
        /// <returns>Coroutine.</returns>
        private IEnumerator TweenCoroutine(SpriteRenderer targetRenderer, SpriteRenderer[] updateRenderers,
            SpriteVisualizerSettings settings, Camera targetCam)
        {
            Transform targetTransform = targetRenderer.transform;
            // Hides the target sprite.
            targetRenderer.enabled = false;

            void SetRendererColor(float value)
            {
                foreach(SpriteRenderer s in updateRenderers)
                {
                    s.color = s.color.SetAlpha(value);
                }
            }

            float fadeTimer = 0;
            // Continually fades in the sprite until it is fully visible.
            while (fadeTimer < settings.FadeInDuration)
            {
                float normalizedTime = fadeTimer / settings.FadeInDuration;

                SetRendererColor(normalizedTime);
                fadeTimer += Time.deltaTime;
                yield return null;
            }
            SetRendererColor(1);

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
            // Plays the graffiti highlight once it has been placed
            GraffitiHighlighter.SetHighlightedGraffiti(targetRenderer);
            // Play the end sound again.
            if (BuildManager.BuildType == BuildType.CityDisplay)
            {
                AudioManager.instance.PlayOneShot(FMODEventsManager.instance.GraffitiDisplay, Vector3.zero);
            }
            Destroy(gameObject);
        }
    }
}