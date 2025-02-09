/*************************************************
Brandon Koederitz
2/8/2025
2/8/2025
A set of mathematical equasions that other scripts can use to simplify themselves.
None.
***************************************************/
using UnityEngine;

namespace GraffitiGala
{
    public static class MathHelpers
    {
        /// <summary>
        /// Converts a vector in cartesian coordinates into polar coordinates.
        /// </summary>
        /// <param name="cartesianCoordinates">The vecctor in cartesian coordiantes to convert.</param>
        /// <returns>
        /// The vector in polar coordinates, with magnitude as the x component and angle as the y component.
        /// </returns>
        public static Vector2 CartesianToPolarCoordinates(Vector2 cartesianCoordinates)
        {
            float angle = VectorToAngle(cartesianCoordinates);
            return new Vector2(cartesianCoordinates.magnitude, angle);
        }

        /// <summary>
        /// Converts an angle in radians into a unit vector in world space pointing in that angle.
        /// </summary>
        /// <param name="angle">The angle in radians to construct a vector in the direction of.</param>
        /// <returns>The constructed vector.</returns>
        public static Vector2 AngleToUnitVector(float angle)
        {
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
        }

        /// <summary>
        /// Converts a vector in absoltue space to a corresponding angle in radians.
        /// </summary>
        /// <param name="vector">The vector to get the angle of.</param>
        /// <returns>The angle of the vector in world space in radians.</returns>
        public static float VectorToAngle(Vector2 vector)
        {
            return Mathf.Atan2(vector.y, vector.x);
        }
    }
}