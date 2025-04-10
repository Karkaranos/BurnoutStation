/*************************************************
Brandon Koederitz
3/5/2025
3/5/2025
Allows buttons to modify this client's line thickness.
***************************************************/
using FishNet.Managing.Scened;
using GraffitiGala.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GraffitiGala.UI
{
    [RequireComponent(typeof(Button))]
    public class ThicknessButton : MonoBehaviour
    {
        [SerializeField] private HighlightAnimator highlightAnim;
        [SerializeField, Tooltip("During what state should this button be interactable.")]
        private ExperienceState[] stateMask;
        [SerializeField] private float thicknessValue;
        //[SerializeField] private bool startAsActive;

        private bool soundAllowed = true;

        private static ThicknessButton currentlyActiveThickness;
        private static ThicknessButton CurrentlyActiveThickness
        {
            get
            {
                return currentlyActiveThickness;
            }
            set
            {
                if (currentlyActiveThickness != null)
                {
                    currentlyActiveThickness.OnLoseActive();
                }
                currentlyActiveThickness = value;
                if (currentlyActiveThickness != null)
                {
                    currentlyActiveThickness.OnBecomeActive();
                }
            }
        }

        ///// <summary>
        ///// If this button is marked as startAsActive, then on start it will set the thickness by default.
        ///// </summary>
        //private void Start()
        //{
        //    ResetThickness();
        //}

        /// <summary>
        /// Sets the thickness of this client's brush.
        /// </summary>
        public void SetThickness()
        {
            if (stateMask.Contains(ExperienceManager.GetState()))
            {
                if (soundAllowed)
                {
                    AudioManager.instance.PlayOneShot(FMODEventsManager.instance.ButtonClick, Vector3.zero);
                }
                else
                {
                    soundAllowed = true;
                }

                NetworkBrush.CurrentThickness = thicknessValue;
                CurrentlyActiveThickness = this;
            }
        }

        public void SetThicknessNoSound()
        {
            soundAllowed = false;
            SetThickness();
        }

        /// <summary>
        /// Resets this object to the default thickness if it is meant to start as the active thickness.
        /// </summary>
        public void ResetThickness()
        {
            CurrentlyActiveThickness = null;
        }

        private void OnLoseActive()
        {
            //Debug.Log("Game object " + gameObject.name + " lost active.");
            highlightAnim.OverrideSelected = false;
        }

        private void OnBecomeActive()
        {
            highlightAnim.OverrideSelected = true;
        }
    }

}