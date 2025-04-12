/*************************************************
Brandon Koederitz
3/12/2025
3/12/2025
Allows the admin to control which players's contributions end up being shown on the finished graffiti.
***************************************************/
using FishNet.Connection;
using GraffitiGala.Drawing;
using System;
using UnityEngine;

namespace GraffitiGala.Admin
{
    public class PlayerHider : MonoBehaviour
    {
        [SerializeField] private PlayerHiderButton[] buttons;
        [SerializeField] private GameObject hiderMenu;
        private int pointerIndex;

        public static event Action<PlayerHider> LineRequest;

        /// <summary>
        /// Provides an array of lines created by one player.
        /// </summary>
        /// <param name="lines">The lines created by that player.</param>
        /// <param name="conn">The network connection that owns the brush that is providing lines.</param>
        public void ProvideLines(MeshBrushTexture[] lines, NetworkConnection conn)
        {
            buttons[pointerIndex].gameObject.SetActive(true);
            buttons[pointerIndex].Setup(lines);
            pointerIndex++;
        }

        /// <summary>
        /// Gives the admin control over the lines that players have drawn.
        /// </summary>
        public void AdministrateDrawing()
        {
            hiderMenu.SetActive(true);
            // Request lines
            pointerIndex = 0;
            // The event system was causing a lot of problems with unsubscribing the event, so I am going to have this
            // script find the objects when it needs to instead.
            //LineRequest?.Invoke(this);
            // Instead of an event, find all mesh net brushes and manually call the ProvideLines function.
            MeshNetBrush[] brushes = FindObjectsOfType<MeshNetBrush>();
            foreach (MeshNetBrush brush in brushes)
            {
                brush.ProvideLines(this);
            }
        }

        /// <summary>
        /// Confirm's the admin's choices and saves the drawing.
        /// </summary>
        public void Confirm()
        {
            ToggleMenu(false);
            GraffitiPhotographer.ScreenshotDrawing();
            AudioManager.instance.PlayOneShot(FMODEventsManager.instance.GraffitiDisplay, Vector3.zero);
            // Move to the waiting state to reset the experience for the next group.
            ExperienceManager.SetState(ExperienceState.Waiting);
        }

        /// <summary>
        /// Denies an entire drawing and prevents it from going on the city at all.
        /// </summary>
        public void Deny()
        {
            ToggleMenu(false);

            // Play Censorshup audio here.

            // Move to the waiting state to reset the experience for the next group.
            ExperienceManager.SetState(ExperienceState.Waiting);
        }

        /// <summary>
        /// Toggles the visibility of the admin menu.
        /// </summary>
        /// <param name="toggle">Whether to enable to disable the menu.</param>
        private void ToggleMenu(bool toggle)
        {
            // Disable the menu and buttons.
            hiderMenu.SetActive(toggle);
            foreach (var button in buttons)
            {
                button.gameObject.SetActive(toggle);
            }
        }
    }
}
