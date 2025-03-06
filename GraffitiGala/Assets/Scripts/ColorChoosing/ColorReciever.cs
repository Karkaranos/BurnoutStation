/*************************************************
Brandon Koederitz
2/14/2025
2/14/2025
Recieves data about a client's colors.
FishNet
***************************************************/
using FishNet;
using FishNet.Transporting;
using GraffitiGala.Drawing;
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
            if(InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.UnregisterBroadcast<ColorData>(RecieveColors);
            }
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
            // Sets the default colors of this client's brush to the first color in the color data's array.
            // Do this from buttons directly so they can set the active state easier.
            //NetworkBrush.CurrentColor = colorData.Colors[0];
            OnRecieveColors?.Invoke(colorData.Colors);
        }
    }
}