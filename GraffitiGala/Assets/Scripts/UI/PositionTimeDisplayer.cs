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

namespace GraffitiGala.UI
{
    public class PositionTimeDisplayer : TimeDisplayer
{
        [SerializeField, Tooltip("The object that will be moved to display the current time.")] 
        private RectTransform displayObject;
        [SerializeField, Tooltip("The anchored position that this element should end up at when the timer is" +
            " finished.")] 
        private Vector3 minPosition;
        [SerializeField, Tooltip("The anchored position that this element should end up at when the timer is" +
            " started")]
        private Vector3 maxPosition;

        /// <summary>
        /// Displays the current time of the timer.
        /// </summary>
        /// <param name="time">The current time of the timer.</param>
        public override void LoadTime(float time)
        {
            Vector3 newPos = Vector3.Lerp(minPosition, maxPosition, time);
            if(displayObject != null )
            {
                displayObject.anchoredPosition = newPos;
            }
        }
    }

}