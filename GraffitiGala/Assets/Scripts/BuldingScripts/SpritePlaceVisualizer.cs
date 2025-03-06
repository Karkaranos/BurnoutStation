/*************************************************
Brandon Koederitz
3/3/2025
3/6/2025
Creates a visually large sprite on screen and scales it down to place it on it's renderer.  Purely a visual script.
***************************************************/
using System.Collections;
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
        public static void PlaceSpriteVisual(SpriteRenderer renderer, SpriteVisualizerSettings settings)
        {
            if (parentTransform == null)
            {
                parentTransform = GameObject.FindGameObjectWithTag(PARENT_TAG).transform;
            }

            GameObject go = new GameObject();
            go.transform.SetParent(parentTransform, false);
            SpriteRenderer sRend = go.AddComponent<SpriteRenderer>();
            sRend.sprite = renderer.sprite;
            SpritePlaceVisualizer visualizer = go.AddComponent<SpritePlaceVisualizer>();
            go.transform.localScale = new Vector3(settings.SpriteMagnification, settings.SpriteMagnification, 1);
            go.transform.localPosition = Vector3.zero;
            visualizer.Tween(renderer, settings);
        }

        /// <summary>
        /// Tweens this object's transform to match the target transform.
        /// </summary>
        /// <param name="renderer">The sprite renderer to copy.</param>
        /// <param name="settings">The settings to use for this sprite tween.</param>
        private void Tween(SpriteRenderer renderer, SpriteVisualizerSettings settings)
        {
            StartCoroutine(TweenCoroutine(renderer, settings));
        }

        /// <summary>
        /// Tweens this object to match the target transform.
        /// </summary>
        /// <param name="renderer">The sprite renderer to copy.</param>
        /// <param name="settings">The settings to use for this sprite tween.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator TweenCoroutine(SpriteRenderer renderer, SpriteVisualizerSettings settings)
        {
            Transform targetTransform = renderer.transform;
            renderer.enabled = false;
            yield return new WaitForSeconds(settings.InitialDelay);
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
            renderer.enabled = true;
            Destroy(gameObject);
        }
    }
}