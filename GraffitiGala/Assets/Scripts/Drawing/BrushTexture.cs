using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*************************************************
Brandon Koederitz
1/26/2025
1/26/2025
Control script for instantiated paint prefabs.
FishNet
***************************************************/
namespace GraffitiGala.Drawing
{
    public class BrushTexture : NetworkBehaviour
    {
        #region vars
        [SerializeField] private SpriteRenderer spriteRenderer;
        #endregion

        #region Methods
        /// <summary>
        /// Configures this paint object when it is spawned by a NetworkedBrush
        /// with pressure changes and a given color.
        /// </summary>
        /// <param name="paintColor">The color of this paint object.</param>
        /// <param name="pressure">
        /// The pressure of the pen when this paint
        /// object was spawned.
        /// </param>
        public void ConfigurePaint(Color paintColor, float pressure)
        {
            spriteRenderer.color = paintColor;
            // handle modifications to the brush based on pressure here.
        }
        #endregion
    }
}