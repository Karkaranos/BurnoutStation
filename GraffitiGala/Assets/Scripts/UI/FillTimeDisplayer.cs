/*************************************************
Brandon Koederitz
2/9/2025
2/9/2025
Displays the current time of the timer by changing the fill of an image.
UI
***************************************************/
using UnityEngine;
using UnityEngine.UI;

namespace GraffitiGala.UI
{
    public class FillTimeDisplayer : TimeDisplayer
{
        [SerializeField] private float pulseFrequency;
        [SerializeField] private float scaleMultiplier;
        [SerializeField, Tooltip("The object that will be moved to display the current time.")] 
        private Image displayObject;
        //[SerializeField, Tooltip("The anchored position that this element should end up at when the timer is" +
        //    " finished.")] 
        //private Vector3 minPosition;
        //[SerializeField, Tooltip("The anchored position that this element should end up at when the timer is" +
        //    " started")]
        //private Vector3 maxPosition;

        //private float maxTime=120;
        //[SerializeField] private GameObject timerHandler;

        private Vector3 baseScale;

        private void Start()
        {
            //maxTime = timerHandler.GetComponent<PlayTimer>().Time;
            baseScale = transform.localScale;
        }


        /// <summary>
        /// Displays the current time of the timer.
        /// </summary>
        /// <param name="time">The current time of the timer.</param>
        public override void LoadTime(float time)
        {
            if (displayObject != null )
            {
                displayObject.fillAmount = time;
            }
        }

        /// <summary>
        /// Pulses the object by modifying it's sized based on a sine wave.
        /// </summary>
        /// <param name="normalizedTime">The normalized time of the timer to use as a reference.</param>
        public override void Pulse(float normalizedTime)
        {
            float pulseValue = (Mathf.Cos(2 * Mathf.PI * normalizedTime * pulseFrequency) + 1) / 2;
            transform.localScale = Vector3.Lerp(baseScale, baseScale * scaleMultiplier, pulseValue);
        }

        /// <summary>
        /// Returns the timer to normal.
        /// </summary>
        public override void ResetValues()
        {
            transform.localScale = baseScale;
        }
    }
}