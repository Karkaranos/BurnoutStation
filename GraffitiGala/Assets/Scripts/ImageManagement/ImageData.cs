/*************************************************
Brandon Koederitz
2/5/2025
2/5/2025
Struct to store information about a saved graffiti image and send that information over the network.
None.
***************************************************/
using FishNet.Broadcast;

namespace GraffitiGala
{
    public struct ImageData : IBroadcast
    {
        public byte[] File { get; set; }
        //public string FileDirectory { get; set; }
        //public string FileName { get; set; }
    }
}