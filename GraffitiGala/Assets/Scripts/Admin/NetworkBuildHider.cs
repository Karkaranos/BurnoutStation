/*************************************************
Brandon Koederitz
2/21/2025
2/21/2025
Hides network elements owned by a given build, including if they are spawned, from all clients.
FishNet
***************************************************/
using FishNet.Object;
using UnityEngine;

namespace GraffitiGala
{
    public class NetworkBuildHider : NetworkBehaviour
    {
        [SerializeField] private HiddenBuilds hiddenInBuilds;

        /// <summary>
        /// Hides this object if the BuildManager doesnt contain it's flag.  Seperate from the default BuildHider
        /// to hide spawned objects on certain builds.
        /// </summary>
        public override void OnStartClient()
        {
            if (BuildManager.CheckBuild(hiddenInBuilds) && base.IsOwner)
            {
                Server_HideObject();
            }
        }
        /// <summary>
        /// Broadcasts to hide this object over the server.
        /// </summary>
        [ServerRpc]
        private void Server_HideObject()
        {
            Client_HideObject();
        }
        /// <summary>
        /// Hides this object on observing clients.
        /// </summary>
        [ObserversRpc(BufferLast = true)]
        private void Client_HideObject()
        {
            Debug.Log("Recieved hide call for a connection.");
            gameObject.SetActive(false);
        }
    }
}
