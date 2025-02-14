/*************************************************
Brandon Koederitz
2/14/2025
2/14/2025
Contains data about a client's colors.
FishNet
***************************************************/
using FishNet.Broadcast;
using UnityEngine;

namespace GraffitiGala.ColorSwitching
{
    public struct ColorData : IBroadcast
    {
        public Color[] Colors { get; set; }
        public int ConnectionID { get; set; }
    }
}