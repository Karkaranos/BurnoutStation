/*************************************************
Brandon Koederitz
2/25/2025
2/25/2025
Exposes a certain set of functions that can affect player brushes to UnityEvents.
***************************************************/
using System;
using UnityEngine;

namespace GraffitiGala.Drawing
{
    public class BrushManager : MonoBehaviour
    {
        public static event Action EnableBrushEvent;
        public static event Action DisableBrushEvent;
        public static event Action ClearLinesEvent;

        /// <summary>
        /// Enables the brush on this client.
        /// </summary>
        public void EnableBrush()
        {
            NetworkBrush.IsEnabled = true;
            EnableBrushEvent?.Invoke();
        }

        /// <summary>
        /// Disables the brush on this client.
        /// </summary>
        public void DisableBrush()
        {
            NetworkBrush.IsEnabled = false;
            DisableBrushEvent?.Invoke();
        }

        /// <summary>
        /// Clears all lines made by this client.
        /// </summary>
        public void ClearLines()
        {
            ClearLinesEvent?.Invoke();
        }
    }
}