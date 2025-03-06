/*************************************************
Brandon Koederitz
2/23/2025
2/23/2025
Tweens an RectTransform between two points depending on if a UI object is highlighted or not. 
Doing it this way to A) Reduce the amount of animation files needed and B) To avoid some of the inconsistencies that 
the animator can have when used with buttons.
***************************************************/
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GraffitiGala.UI
{
    public class HighlightTweener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region CONSTS
        private const float SNAP_DISTANCE = 0.01f;
        private const float LERP_VALUE = 0.5f;
        #endregion

        [SerializeField] private RectTransform animatedObject;
        [SerializeField] private Vector2 unselectedAnchoredPosition;
        [SerializeField] private Vector2 selectedAnchoredPosition;
        [SerializeField] private float tweenSpeed;

        // When this value is true, the object will always be tweened to the selected position.
        private bool overrideSelected = false;
        public bool OverrideSelected
        {
            get
            {
                return overrideSelected;
            }
            set
            {
                // Always ensure we tween to the correct position when override selected is set.
                overrideSelected = value;
                Debug.Log("Override Selected set to " +  value);
                AutoTween();
            }
        }

        private bool isHighlighted;

        private Coroutine tweenRoutine;

        /// <summary>
        /// Tweens the animated object to the selected anchored position when the pointer enters this UI object.
        /// </summary>
        /// <param name="eventData">Unused.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            isHighlighted = true;
            //Tween(selectedAnchoredPosition);
            AutoTween();
        }
        /// <summary>
        /// Tweens the animated object to the unselected anchored position when the pointer enters this UI object.
        /// </summary>
        /// <param name="eventData">Unused.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            isHighlighted = false;
            //Tween(unselectedAnchoredPosition);
            AutoTween();
        }

        /// <summary>
        /// Automatically tweens the image to the correct location based on it's highlighted status and if it is being overwritten.
        /// </summary>
        private void AutoTween()
        {
            bool shouldBeRaised = isHighlighted | overrideSelected;
            Vector2 targetPos = shouldBeRaised ? selectedAnchoredPosition : unselectedAnchoredPosition;
            Tween(targetPos);
        }

        /// <summary>
        /// Starts tweening the animated object to a given target position.
        /// </summary>
        /// <param name="targetPos">The target position to tween to.</param>
        private void Tween(Vector2 targetPos)
        {
            if (tweenRoutine != null)
            {
                StopCoroutine(tweenRoutine);
            }
            tweenRoutine = StartCoroutine(TweenRoutine(targetPos));
        }

        /// <summary>
        /// Tweens a UI element to a given anchored position.
        /// </summary>
        /// <remarks>
        /// Uses a LERP for smoothing.
        /// </remarks>
        /// <param name="targetPosition">The anchored position to tween to.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator TweenRoutine(Vector2 targetPosition)
        {
            while (Vector2.Distance(animatedObject.anchoredPosition, targetPosition) > SNAP_DISTANCE)
            {
                // Tweens the object to the target position with a LERP.
                float blend = 1 - Mathf.Pow(LERP_VALUE, tweenSpeed * Time.deltaTime);
                Vector2 newPos = Vector2.Lerp(animatedObject.anchoredPosition, targetPosition, blend);
                animatedObject.anchoredPosition = newPos;
                yield return null;
            }
            // Snaps the object to the target position.
            animatedObject.anchoredPosition = targetPosition;
            tweenRoutine = null;
        }
    }

}