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
        #region vars
        [SerializeReference] private LineRenderer lineRenderer;
        [SerializeReference, Tooltip("The width of this line when the player is applying" +
            " the minimum amount of pressure to the pen.  If this value is larger than" +
            " Max Pressure Width, then the line will appear larger when less pressure is applied.")] 
        private float minPressureWidth;
        [SerializeReference, Tooltip("The width of this line when the player is applying" +
            " the maximum amount of pressure to the pen.  If this value is smaller than" +
            " Min Pressure Width, then the line will appear smaller when more pressure is applied.")] 
        private float maxPressureWidth;

        private bool isNetworked;
        #endregion

        #region Properties
        internal LineRenderer LR
        {
            get
            {
                return lineRenderer;
            }
        }
        #endregion

        /// <summary>
        /// Configures this object with values.
        /// </summary>
        /// <param name="color">The color this line should be.</param>
        public void Configure(Color color)
        {
            // Updates the line renderer's color.
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

        private void OnDisable()
        {
            Debug.Log("I was disabled.");
        }

        /// <summary>
        /// Configures this object with values when it is spawned over the server.
        /// </summary>
        /// <param name="color">The color this line should be.</param>
        [ObserversRpc]
        public void ObserverConfigure(Color color)
        {
            Configure(color);
        }

        /// <summary>
        /// Sets up this line to send information about it's updates over the network.
        /// </summary>
        /// <param name="clientReferenceLine">
        /// The line displayed on the client that this line should update to match.
        /// </param>
        public void InitializeAsNetworked(LRBrushTexture clientReferenceLine)
        {
            // Load point values here.
            // Gets the number of and locations of all the positions that the 
            // clientReverenceLine had in it's line renderer.
            int posCount = clientReferenceLine.LR.positionCount;
            Vector3[] positions = new Vector3[posCount];
            clientReferenceLine.LR.GetPositions(positions);
            // Sets this line renderer to reflect the client line renderer.
            SetPositions(positions);
            // Sets the line rendere to reflec the client line renderer for other clients across the network.
            InitNetworkedServer(positions);

            isNetworked = true;
            // Destroys the client reference line because it is no longer needed.
            // This line shall now be updated in it's place.
            Destroy(clientReferenceLine.gameObject);
        }

        /// <summary>
        /// Sets this object's line renderer positions to a given array of positions.
        /// </summary>
        /// <param name="positions">The positions to set to this line renderer.</param>
        private void SetPositions(Vector3[] positions)
        {
            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }


        /// <summary>
        /// Initializes a pre-established set of positions for this object's line renderer across the network.
        /// </summary>
        /// <param name="positions">The positions to set for the line renderer.</param>
        [ServerRpc]
        private void InitNetworkedServer(Vector3[] positions)
        {
            InitNetworkedObserver(positions);
        }
        /// <summary>
        /// Initializes a pre-established set of positions for this object's line renderer for this client.
        /// </summary>
        /// <param name="positions">The positions to set for the line renderer.</param>
        [ObserversRpc(ExcludeOwner = true)]
        private void InitNetworkedObserver(Vector3[] positions)
        {
            SetPositions(positions);
        }


        /// <summary>
        /// Adds a point to this line.
        /// </summary>
        /// <param name="position">The position to add the point at.</param>
        /// <param name="pressure">The pressure of the pen when this point is added.</param>
        public void AddPoint(Vector3 position, float pressure)
        {
            // Adds a point to this line on the client side.
            AddPointToThis(position, pressure);
            // If this line is set up to work over the network, then send
            // an RPC to the server that will add a point to all other clients.
            if(isNetworked)
            {
                AddPointServer(position, pressure);
            }
        }

        /// <summary>
        /// Adds a point to this line on the client side.
        /// </summary>
        /// <param name="position">The position to add a point at.</param>
        /// <param name="pressure">The pressure of the pen when this point was added.</param>
        private void AddPointToThis(Vector3 position, float pressure)
        {
            // Adds a new position to the LineRenderer
            lineRenderer.positionCount += 1;
            // Sets the position of the newly created point to be equal to the passed in position.
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
            // Handle pressure here.
        }

        /// <summary>
        /// Adds a new point to this line on the server.
        /// </summary>
        /// <param name="position">The position to spawn the point at.</param>
        /// <param name="pressure">The pressure that determines the width of the line.</param>
        [ServerRpc]
        private void AddPointServer(Vector3 position, float pressure)
        {
            AddPointClient(position, pressure);
        }

        /// <summary>
        /// Adds a new point to this line.
        /// </summary>
        /// <param name="position">The position to spawn the point at.</param>
        /// <param name="pressure">The pressure that determines the width of the line.</param>
        [ObserversRpc(ExcludeOwner = true)]
        private void AddPointClient(Vector3 position, float pressure)
        {
            AddPointToThis(position, pressure);
        }
    }
}