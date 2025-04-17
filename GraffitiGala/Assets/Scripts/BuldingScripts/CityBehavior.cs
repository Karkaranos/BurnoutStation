/*************************************************
Brandon Koederitz
4/16/2025
4/16/2025
Base class for objects that scroll in the main city view.
***************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraffitiGala.City
{
    public class CityBehavior : MonoBehaviour
    {
        [field: SerializeField, Tooltip("Building Width, in Unity Units")]
        public float ObjectWidth { get; protected set; }
        [field: SerializeField, Tooltip("Building Height, in Unity Units")]
        public float ObjectHeight { get; protected set; }
    }
}