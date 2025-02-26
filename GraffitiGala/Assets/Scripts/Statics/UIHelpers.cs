/*************************************************
Brandon Koederitz
2/22/2025
2/26/2025
Set of helper functions for the UI
***************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GraffitiGala
{
    public static class UIHelpers
    {
        /// <summary>
        /// Checks if a specific position is over a UI element.
        /// </summary>
        /// <param name="pos">The position in screen space to check.</param>
        /// <returns>Whether the position is over a UI object.</returns>
        public static bool IsPositionOverUI(Vector2 pos)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = pos;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].gameObject.CompareTag("IgnorePointer"))
                {
                    results.RemoveAt(i);
                    i--;
                }
            }
            return results.Count > 0;
        }

    }
}