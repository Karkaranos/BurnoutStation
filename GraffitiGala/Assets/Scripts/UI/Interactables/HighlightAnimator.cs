/*************************************************
Brandon Koederitz
3/12/2025
3/12/2025
Animates a button on selection.  Done manually to A) allow for animating via script instead of having to use the unity 
animator. B) to make it so that animations only play during certain experience states.
***************************************************/
using UnityEngine;
using UnityEngine.EventSystems;

namespace GraffitiGala.UI
{
    public abstract class HighlightAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, Tooltip("During what state should this object animate itself.")]
        protected ExperienceState stateMask;
        [SerializeField, Tooltip("The game object that indicates that this button is the active button.")]
        private GameObject overrideObject;

        protected bool isHighlighted;

        // When this value is true, the object will always be tweened to the selected position.
        protected bool overrideSelected = false;
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
                // Enable and disable the game object that indicates this button is active due to an outside override.
                if (overrideObject != null)
                {
                    overrideObject.SetActive(value);
                }
                //Debug.Log("Override Selected set to " + value);
                Evaluate();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHighlighted = true;
            Evaluate();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHighlighted = false;
            Evaluate();
        }

        /// <summary>
        /// Evaluates the state that this animator should be in based on the isHighlighted and overrideSelected values.
        /// </summary>
        protected abstract void Evaluate();
    }
}