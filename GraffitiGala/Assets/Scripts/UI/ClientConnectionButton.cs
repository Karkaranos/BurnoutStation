/*************************************************
Brandon Koederitz
4/1/2025
4/1/2025
Controls buttons that clients use to connect to the server.
FishNet
***************************************************/
using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;

namespace GraffitiGala.UI
{
    public class ClientConnectionButton : MonoBehaviour
    {
        //[SerializeField] private GameObject connectionButtonObject;
        protected NetworkManager networkManager;
        protected LocalConnectionState state = LocalConnectionState.Stopped;

        /// <summary>
        /// Finds and stores a reference to the network manager.
        /// </summary>
        private void Awake()
        {
            networkManager = FindObjectOfType<NetworkManager>();

            if (networkManager == null )
            {
                Debug.Log("Network Manager not found");
            }
            else
            {
                // Subscribe to NetworkManager events to handle changes in state
                networkManager.ClientManager.OnClientConnectionState += HandleConnectionStateChange;
            }
        }

        /// <summary>
        /// Unsubscribe events.
        /// </summary>
        private void OnDestroy()
        {
            if (networkManager != null )
            {
                networkManager.ClientManager.OnClientConnectionState -= HandleConnectionStateChange;
            }
        }

        /// <summary>
        /// Changes this button's state to match the connection state of the network,
        /// </summary>
        /// <param name="obj">A set of Args passed by the client manager.</param>
        private void HandleConnectionStateChange(ClientConnectionStateArgs obj)
        {
            state = obj.ConnectionState;
            // Hides this button if the connection is not stopped, but re-enables it if the connection returns to the
            // stopped state.
            //if (connectionButtonObject != null)
            //{
            //    if (state == LocalConnectionState.Stopped)
            //    {
            //        connectionButtonObject.SetActive(true);
            //    }
            //    else
            //    {
            //        connectionButtonObject.SetActive(false);
            //    }
            //}
            if (state == LocalConnectionState.Stopped)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Starts/Stops the connection to the server.
        /// </summary>
        public void OnClick()
        {
            if (networkManager == null)
                return;

            if (state != LocalConnectionState.Stopped)
                networkManager.ClientManager.StopConnection();
            else
                networkManager.ClientManager.StartConnection();
        }
    }
}
