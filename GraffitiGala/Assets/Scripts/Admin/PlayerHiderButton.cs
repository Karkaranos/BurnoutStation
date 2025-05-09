/*************************************************
Brandon Koederitz
3/12/2025
3/12/2025
Controls buttons that hide player contributions.
***************************************************/
using GraffitiGala.Drawing;
using System.Linq;
using TMPro;
using UnityEngine;

namespace GraffitiGala.Admin
{
    public class PlayerHiderButton : MonoBehaviour
    {
        #region CONSTS
        private const string PLAYER_NAME = "Player";
        private const string HIDE_TEXT = "Hide";
        private const string SHOW_TEXT = "Show";
        #endregion
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private int playerNumber;
        private MeshBrushTexture[] lines;
        private bool isVisible;

        /// <summary>
        /// Toggles the visibility of the lines this player has made.
        /// </summary>
        public void ToggleLines()
        {
            FindObjectOfType<PlayerHider>().PlayersHidden += isVisible == true ? 1 : -1;
            isVisible = !isVisible;
            foreach (var line in lines)
            {
                if (line == null) { continue; }
                line.gameObject.SetActive(isVisible);
            }
            UpdateButtonText();
        }

        /// <summary>
        /// Updates the button text to match the current status of the player's lines.
        /// </summary>
        private void UpdateButtonText()
        {
            string text = isVisible ? HIDE_TEXT : SHOW_TEXT;
            text = $"{text} {PLAYER_NAME} {playerNumber}";
            buttonText.text = text;
        }

        /// <summary>
        /// Sets up this button for a newly finished experience round.
        /// </summary>
        /// <param name="lines">The lines that were drawn by this player.</param>
        public void Setup(MeshBrushTexture[] lines)
        {
            //Debug.Log(lines.Length);
            this.lines = lines;
            isVisible = true;
            UpdateButtonText();
        }
    }
}