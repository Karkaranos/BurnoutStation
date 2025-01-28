using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*************************************************
Brandon Koederitz
1/27/2025
1/27/2025
Control script for instantiated paint prefabs that use the LR (Line Renderer) system.
FishNet
***************************************************/

namespace GraffitiGala.Drawing
{
    [RequireComponent(typeof(LineRenderer))]
    public class LRBrushTexture : NetworkBehaviour
    {
        [SerializeReference] private LineRenderer lineRenderer;

        /// <summary>
        /// Configures this object with values when it is spawned over the server.
        /// </summary>
        /// <param name="color">The color this line should be.</param>
        [ObserversRpc]
        public void Configure(Color color)
        {
            // Updates the line renderer's color.
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

        /// <summary>
        /// Adds a new point to this line on the server.
        /// </summary>
        /// <param name="position">The position to spawn the point at.</param>
        /// <param name="pressure">The pressure that determines the width of the line.</param>
        [ServerRpc]
        public void AddPoint(Vector3 position, float pressure)
        {
            AddPointClient(position, pressure);
        }

        /// <summary>
        /// Adds a new point to this line.
        /// </summary>
        /// <param name="position">The position to spawn the point at.</param>
        /// <param name="pressure">The pressure that determines the width of the line.</param>
        [ObserversRpc]
        public void AddPointClient(Vector3 position, float pressure)
        {
            // Adds a new position to the LineRenderer
            lineRenderer.positionCount += 1;
            // Sets the position of the newly created point to be equal to the passed in position.
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
            // Handle pressure here.
        }
    }
}