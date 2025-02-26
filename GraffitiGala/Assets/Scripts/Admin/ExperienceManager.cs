/*************************************************
Brandon Koederitz
2/23/2025
2/23/2025
Broadcasts events that manage the beginning and ending of an experience.
FishNet, NaughtyAttributes
***************************************************/
using FishNet;
using FishNet.Object;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace GraffitiGala
{
    public class ExperienceManager : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField, Tooltip("The number of clients that need to ready before the experience starts.")] 
        private int readyNumber = 3;
        [Header("Server Events")]
        [SerializeField] private UnityEvent OnExperienceStartServer;
        [SerializeField] private UnityEvent OnExperienceEndServer;
        [Header("Client Events")]
        [SerializeField] private UnityEvent OnExperienceStartClient;
        [SerializeField] private UnityEvent OnExperienceEndClient;



        /// <summary>
        /// Makes the host the owner of this object.
        /// </summary>
        public override void OnStartClient()
        {
            if (base.IsHostStarted && !base.IsOwner)
            {
                this.GiveOwnership(InstanceFinder.ClientManager.Connection);
            }
        }

        #region Beginning and End functions
        /// <summary>
        /// Begins the experience for all clients.
        /// </summary>
        [Button]
        private void BeginExperience()
        {
            // Only the host is allowed to call BeginExperience.
            if (base.IsHostStarted)
            {
                OnExperienceStartServer?.Invoke();
                OnExperienceStartClient?.Invoke();
                Server_BeginExperience();
            }
        }

        /// <summary>
        /// Begisn the experience for all clients.
        /// </summary>
        [ServerRpc]
        private void Server_BeginExperience()
        {
            Client_BeginExperience();
        }

        /// <summary>
        /// Begins the experience for this client.
        /// </summary>
        [ObserversRpc(ExcludeOwner = true)]
        private void Client_BeginExperience()
        {
            OnExperienceStartClient?.Invoke();
        }

        /// <summary>
        /// Ends the experienc for all clients.
        /// </summary>
        [Button]
        private void EndExperience()
        {
            // Only the host is allowed to call EndExperience.
            if (base.IsHostStarted)
            {
                OnExperienceEndServer?.Invoke();
                OnExperienceEndClient?.Invoke();
                Server_EndExperience();
            }
        }

        /// <summary>
        /// Ends the experience for all clients.
        /// </summary>
        [ServerRpc]
        private void Server_EndExperience()
        {
            Client_EndExperience();
        }

        /// <summary>
        /// Ends the experience for this client.
        /// </summary>
        [ObserversRpc(ExcludeOwner = true)]
        private void Client_EndExperience()
        {
            OnExperienceEndClient?.Invoke();
        }
        #endregion
    }

}