/*************************************************
Brandon Koederitz
2/16/2025
2/16/2025
A request from a client to the server to give it colors after it joins late.
FishNet
***************************************************/
using FishNet.Broadcast;
using FishNet.Connection;

namespace GraffitiGala.ColorSwitching
{
    public struct ColorRequest : IBroadcast
    {
        // No information needed here since network connection is automatically passed through the broadcast.
    }
}