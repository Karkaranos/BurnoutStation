/*************************************************
Brandon Koederitz
2/9/2025
2/9/2025
Displays the current time of the timer by moving a UI element.
UI
***************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   

namespace GraffitiGala.UI
{
    public class PositionTimeDisplayer : TimeDisplayer
{
        [SerializeField, Tooltip("The object that will be moved to display the current time.")] 
        private Image displayObject;
        [SerializeField, Tooltip("The anchored position that this element should end up at when the timer is" +
            " finished.")] 
        private Vector3 minPosition;
        [SerializeField, Tooltip("The anchored position that this element should end up at when the timer is" +
            " started")]
        private Vector3 maxPosition;

        private float maxTime=120;
        [SerializeField] private GameObject timerHandler;

        private void Start()
        {
            maxTime = timerHandler.GetComponent<PlayTimer>().Time;
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
    }

}