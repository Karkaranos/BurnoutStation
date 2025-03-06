/*************************************************
Brandon Koederitz
3/3/2025
3/3/2025
Creates a visually large sprite on screen and scales it down to place it on it's renderer.  Purely a visual script.
***************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraffitiGala.City
{
    public class SpritePlaceVisualizer : MonoBehaviour
    {
        /// <summary>
        /// Plays a visual where a large sprite is created then it shrinks down to match a given sprite renderer.
        /// </summary>
        /// <param name="renderer">The sprite renderer to copy.</param>
        /// <param name="spriteMagnification">How much larger the sprite shoudld appear when being displayed compared to it's actual size.</param>
        /// <param name="tweenTime">The amount of time the sprite should take to reach it's position.</param>
        /// <param name="initialDelay">The amount of initial delay before the sprite visual begins to tween.</param>
        public static void PlaceSpriteVisual(SpriteRenderer renderer, float spriteMagnification, float tweenTime, float initialDelay = 0f)
        {
            GameObject go = new GameObject();
            SpriteRenderer sRend = go.AddComponent<SpriteRenderer>();
            sRend.sprite = renderer.sprite;
            SpritePlaceVisualizer visualizer = go.AddComponent<SpritePlaceVisualizer>();
            go.transform.localScale = new Vector3(spriteMagnification, spriteMagnification, 1);
            go.transform.localPosition = Vector3.zero;
            visualizer.Tween(go.transform, tweenTime, initialDelay);
        }

        /// <summary>
        /// Tweens this object's transform to match the target transform.
        /// </summary>
        /// <param name="targetTransform">The transform that this transform should match.</param>
        /// <param name="tweenTime">The amount of time this tween should take.</param>
        /// <param name="initialDelay">The initial delay before this tween starts.</param>
        private void Tween(Transform targetTransform, float tweenTime, float initialDelay)
        {
            StartCoroutine(TweenCoroutine(targetTransform, tweenTime, initialDelay));
        }

        /// <summary>
        /// Tweens this object to match the target transform.
        /// </summary>
        /// <param name="targetTransform">The transform that this transform should match.</param>
        /// <param name="tweenTime">The amount of time this tween should take.</param>
        /// <param name="initialDelay">The initial delay before this tween starts.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator TweenCoroutine(Transform targetTransform, float tweenTime, float initialDelay)
        {
            yield return new WaitForSeconds(initialDelay);

        }
    }
}