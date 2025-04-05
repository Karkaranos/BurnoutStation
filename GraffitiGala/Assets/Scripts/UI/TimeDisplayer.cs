/*************************************************
Brandon Koederitz
2/9/2025
2/9/2025
Displays the current time of the timer
UI
***************************************************/
using UnityEngine;

namespace GraffitiGala.UI
{
    public abstract class TimeDisplayer : MonoBehaviour
    {
        public abstract void LoadTime(float normalizedTime);

        public abstract void Pulse(float normalizedTime);

        public abstract void ResetValues();
    }

}