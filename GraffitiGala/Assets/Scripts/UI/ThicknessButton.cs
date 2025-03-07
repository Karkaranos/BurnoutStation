/*************************************************
Brandon Koederitz
3/5/2025
3/5/2025
Allows buttons to modify this client's line thickness.
***************************************************/
using GraffitiGala.Drawing;
using UnityEngine;
using UnityEngine.UI;

namespace GraffitiGala.UI
{
    [RequireComponent(typeof(Button))]
    public class ThicknessButton : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private float thicknessValue;
        [SerializeField] private bool startAsActive;

        private Sprite savedSprite;

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

        /// <summary>
        /// If this button is marked as startAsActive, then on start it will set the thickness by default.
        /// </summary>
        private void Start()
        {
            if (startAsActive)
            {
                SetThickness();
            }
        }

        /// <summary>
        /// Sets the thickness of this client's brush.
        /// </summary>
        public void SetThickness()
        {
            NetworkBrush.CurrentThickness = thicknessValue;
            CurrentlyActiveThickness = this;
        }

        private void OnLoseActive()
        {
            image.sprite = savedSprite;
        }

        private void OnBecomeActive()
        {
            if (savedSprite == null)
            {
                savedSprite = image.sprite;
            }
            image.sprite = activeSprite;
        }
    }

}