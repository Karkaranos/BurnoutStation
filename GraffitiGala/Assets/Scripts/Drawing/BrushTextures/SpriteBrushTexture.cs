/*************************************************
Brandon Koederitz
1/26/2025
1/26/2025
Control script for instantiated paint prefabs.
FishNet
***************************************************/

using FishNet.Object;
using UnityEngine;

namespace GraffitiGala.Drawing
{
    public class SpriteBrushTexture : NetworkBehaviour
    {
        #region vars
        [SerializeReference] private SpriteRenderer spriteRenderer;
        #endregion

        #region Methods
        /// <summary>
        /// Configures this paint object when it is instantiated so that it
        /// shows up correclty to the server.
        /// </summary>
        /// <param name="paintColor">The color of this paint object.</param>
        /// <param name="pressure">
        /// The pressure of the pen when this paint
        /// object was spawned.
        /// </param>
        public void PreconfigurePaint(Color paintColor, float pressure)
        {
            spriteRenderer.color = paintColor;
            // handle modifications to the brush based on pressure here.
            // Enables the GameObject so that players can only see the paint after
            // it has been configured.
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Configures this paint object when it is spawned by the server
        /// across the network.
        /// </summary>
        /// <param name="paintColor">The color of this paint object.</param>
        /// <param name="pressure">
        /// The pressure of the pen when this paint
        /// object was spawned.
        /// </param>
        [ObserversRpc]
        public void ConfigurePaint(Color paintColor, float pressure)
        {
            spriteRenderer.color = paintColor;
            // handle modifications to the brush based on pressure here.
            // Enables the GameObject so that players can only see the paint after
            // it has been configured.
            gameObject.SetActive(true);
        }
        #endregion
    }
}