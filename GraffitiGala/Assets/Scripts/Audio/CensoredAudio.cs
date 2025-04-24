using FishNet;
using FishNet.Transporting;
using UnityEngine;

namespace GraffitiGala.Admin
{
    public class CensoredAudio : MonoBehaviour
    {
        /// <summary>
        /// Register the SaveImage function to a client manager broadcast.
        /// </summary>
        private void OnEnable()
        {
            if (FindObjectOfType<BuildManager>().BuildTypeRef == BuildType.CityDisplay)
            {
                InstanceFinder.ClientManager.RegisterBroadcast<CensoredAudioData>(PlaySound);
            }
        }
        /// <summary>
        /// Register the SaveImage function from the client manager broadcast.
        /// </summary>
        private void OnDisable()
        {
            if (InstanceFinder.ClientManager != null && FindObjectOfType<BuildManager>().BuildTypeRef == BuildType.CityDisplay)
            {
                InstanceFinder.ClientManager.UnregisterBroadcast<CensoredAudioData>(PlaySound);
            }
        }

        private void PlaySound(CensoredAudioData audioData, Channel channel)
        {
            if (!(FindObjectOfType<BuildManager>().BuildTypeRef == BuildType.CityDisplay)) { return; }
            if (audioData.IsCensored)
            {
                AudioManager.instance.PlayCensored(Vector3.zero);
            }
            else
            {
                AudioManager.instance.PlayApproval(Vector3.zero);
            }
        }
    }
}