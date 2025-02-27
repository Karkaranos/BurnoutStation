/*************************************************
Brandon Koederitz
2/26/2025
2/526/2025
Struct to store information about a given prompt to send over the network
FishNet
***************************************************/
using FishNet.Broadcast;

namespace GraffitiGala
{
    public struct PromptData : IBroadcast
    {
        public string Prompt { get; set; }
        //public string FileDirectory { get; set; }
        //public string FileName { get; set; }
    }
}