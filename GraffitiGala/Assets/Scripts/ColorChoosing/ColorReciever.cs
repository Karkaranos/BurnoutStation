/*************************************************
Brandon Koederitz
2/14/2025
2/14/2025
Recieves data about a client's colors.
FishNet
***************************************************/
using FishNet;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace GraffitiGala.ColorSwitching
{
    public class ColorReciever : MonoBehaviour
    {
        [SerializeField] private UnityEvent<Color[]> OnRecieveColors;

        /// <summary>
        /// Register the SaveImage function to a client manager broadcast.
        /// </summary>
        private void OnEnable()
        {
            InstanceFinder.ClientManager.RegisterBroadcast<ColorData>(RecieveColors);
        }
        /// <summary>
        /// Register the SaveImage function from the client manager broadcast.
        /// </summary>
        private void OnDisable()
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<ColorData>(RecieveColors);
        }

        /// <summary>
        /// Recieves colors for this client from the server and broadcasts them over an event.
        /// </summary>
        /// <param name="colorData"></param>
        /// <param name="channel"></param>
        private void RecieveColors(ColorData colorData, Channel channel)
        {
            // Double check that this set of color data was intended for this client.
            if(colorData.ConnectionID != InstanceFinder.ClientManager.Connection.ClientId)
            {
                Debug.LogError("Client recieved color data with an incorrect connection ID");
                return;
            }
            //Debug.LogError("RecievedColors");
            OnRecieveColors?.Invoke(colorData.Colors);
        }
    }
}