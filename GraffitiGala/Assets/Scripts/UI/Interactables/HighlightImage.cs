/*************************************************
Brandon Koederitz
3/12/2025
3/12/2025
Changes the image of a button's image component when it is highlighted.
***************************************************/
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GraffitiGala.UI
{
    public class HighlightImage : HighlightAnimator
    {
        [SerializeField] private Image image;
        [SerializeField] private Sprite activeSprite;
        private Sprite savedSprite;


        private void Awake()
        {
            savedSprite = image.sprite;
        }

        /// <summary>
        /// Sets the image's sprite based on if the button should be highlighted or not.
        /// </summary>
        protected override void Evaluate()
        {
            bool shouldBeActive = (isHighlighted | overrideSelected) & stateMask.Contains(ExperienceManager.GetState());
            if (shouldBeActive)
            {
                image.sprite = activeSprite;
            }
            else
            {
                image.sprite = savedSprite;
            }
        }
    }
}