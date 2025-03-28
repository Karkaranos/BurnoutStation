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
            hiderMenu.SetActive(false);
            foreach (var button in buttons)
            {
                button.gameObject.SetActive(false);
            }
            GraffitiPhotographer.ScreenshotDrawing();
            AudioManager.instance.PlayOneShot(FMODEventsManager.instance.GraffitiDisplay, Vector3.zero);
            // Move to the waiting state to reset the experience for the next group.
            ExperienceManager.SetState(ExperienceState.Waiting);
        }
    }
}
