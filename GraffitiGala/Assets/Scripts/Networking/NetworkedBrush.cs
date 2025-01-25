using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using UnityEngine.InputSystem;
using NaughtyAttributes;

/*************************************************
Brandon Koederitz
1/24/2025
1/24/2025
Networks a brush object to be visually updated for all users.
FishNet
***************************************************/

namespace GraffitiGala.Networking
{
    public class NetworkedBrush : NetworkBehaviour
    {
        #region vars

        #endregion

        #region Methods
        /// <summary>
        /// Sets up this as a networked object.
        /// </summary>
        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

        }

        /// <summary>
        /// Tests if this object can send and recieve RPCs across the network correctly.
        /// </summary>
        [Button]
        private void SendTestRPC()
        {
            SendInput();
        }

        /// <summary>
        /// Tests if this object's transform will automatically sync correctly
        /// with the NetworkTransform component.
        /// </summary>
        [Button]
        private void TestTransformMovement()
        {
            transform.position = transform.position + Vector3.down;
        }

        //TODO: Need to fill in params for send and recieve input when we know what
        //kind of information we will need to send across the server.

        /// <summary>
        /// Sends a given 
        /// </summary>
        [ServerRpc]
        public void SendInput()
        {
            RecieveInput();
        }

        /*RPC Notes:
         * When a server RPC calls an Observer RPC, only the object that the Observer 
         * RPC is being called on will trigger.  It does not act like an event or
         * message broadcasting to all Network Observers.
         */
        /// <summary>
        /// Handles this networked object recieving an input from the server sent
        /// by the player.
        /// </summary>
        [ObserversRpc]
        public void RecieveInput()
        {
            //recievingObject.transform.position = recievingObject.transform.position + Vector3.up;
            // Here to test if the networked objects are recieving RPCs correctly.
            transform.position = transform.position + Vector3.up;
        }
        #endregion
    }

}