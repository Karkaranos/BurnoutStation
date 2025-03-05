/*************************************************
Brandon Koederitz
2/14/2025
2/14/2025
Contains data about the colors players can use.
***************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraffitiGala.ColorSwitching
{
    [CreateAssetMenu(fileName = "ColorChoices", menuName = "Graffiti Gala/Color Choices")]
    public class ColorChoices : ScriptableObject
    {
        [field: SerializeField] public Color[] Colors { get; private set; }
    }
}