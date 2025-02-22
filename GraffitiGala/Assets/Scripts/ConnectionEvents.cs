/*************************************************
Brandon Koederitz
2/22/2025
2/22/2025
Simple broadcaster of events to manage behaviour that should trigger on server or client connections.
FishNet
***************************************************/
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;

namespace GraffitiGala
{
    public class ConnectionEvents : NetworkBehaviour
    {
        [SerializeField] private UnityEvent StartClient;
        [SerializeField] private UnityEvent StopClient;
        [SerializeField] private UnityEvent StartServer;
        [SerializeField] private UnityEvent StopServer;

        public override void OnStartClient()
        {
            StartClient?.Invoke();
        }

        public override void OnStopClient()
        {
            StopClient?.Invoke();
        }

        public override void OnStartServer()
        {
            StartServer?.Invoke();
        }

        public override void OnStopServer()
        {
            StopServer?.Invoke();
        }
    }
}