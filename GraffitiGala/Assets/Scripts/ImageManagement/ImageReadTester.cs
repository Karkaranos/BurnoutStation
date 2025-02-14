/*************************************************
Brandon Koederitz
2/5/2025
2/5/2025
Temp script to test if loading saved images as sprites works.
None
***************************************************/
using UnityEngine;

namespace GraffitiGala
{
    public class ImageReadTester : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer sRenderer;

        /// <summary>
        /// Updates the sprite renderers's sprite to a png file at the specified file path.
        /// </summary>
        /// <param name="filePath">The file path to get the png image from</param>
        public void UpdateSprite(string filePath)
        {
            sRenderer.sprite = ImageManagement.LoadSprite(filePath, new Vector2(0.5f, 0.5f), 256f);
            Debug.Log("Should be calling a function on BuildingManager");
        }
    }
}