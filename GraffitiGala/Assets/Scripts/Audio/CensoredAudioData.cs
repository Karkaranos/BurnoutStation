using FishNet.Broadcast;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraffitiGala.Admin
{
    public struct CensoredAudioData : IBroadcast
    {
        public bool IsCensored { get; set; }
    }
}