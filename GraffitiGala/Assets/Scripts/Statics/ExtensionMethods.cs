/*************************************************
Brandon Koederitz
3/5/2025
3/5/2025
Scrolls buildings past the camera and wraps them around.
***************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraffitiGala
{
    public static class ExtensionMethods
    {

        /// <summary>
        /// Checks if a given sprite renderer is within the planes of a given camera.
        /// </summary>
        /// <param name="cam">The camera to check if the sprite is in the planes of.</param>
        /// <param name="rend">The renderer to check the bounds of.</param>
        /// <returns>Whether the renderer's bounds appear within the camera.</returns>
        public static bool CheckObjectInCamera(this Camera cam, Renderer rend)
        {
            return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(cam), rend.bounds);
        }

        /// <summary>
        /// Sets the alpha value of a color.
        /// </summary>
        /// <param name="color">The color to modify.</param>
        /// <param name="alpha">The alpha value to set.</param>
        public static Color SetAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }

}