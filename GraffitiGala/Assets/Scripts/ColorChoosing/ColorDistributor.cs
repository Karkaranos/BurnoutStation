/*************************************************
Brandon Koederitz
2/14/2025
2/14/2025
Sends possible colors to clients across the network.
FishNet
***************************************************/
using FishNet;
using FishNet.Connection;
using System.Collections.Generic;
using UnityEngine;

namespace GraffitiGala.ColorSwitching
{
    public class ColorDistributor : MonoBehaviour
    {
        [SerializeField] private ColorChoices colorChoices;
        [SerializeField, Tooltip("The number of colors that are available.")] private int colorNumber;

        /// <summary>
        /// Sends a unique set of colors to each client.
        /// </summary>
        public void SendColors()
        {
            List<Color> possibleColors = new();
            possibleColors.AddRange(colorChoices.Colors);
            // Get all clients connected to this server.
            Dictionary<int, NetworkConnection> clients = InstanceFinder.ServerManager.Clients;
            foreach (NetworkConnection conn in clients.Values)
            {
                if (conn == null) { continue; }
                Color[] clientColors = new Color[colorNumber];
                for (int i = 0; i < clientColors.Length; i++)
                {
                    // Gets a random color from the colorChoices list and removes that color as a possible choice.
                    int colorIndex = Random.Range(0, possibleColors.Count);
                    Color col = possibleColors[colorIndex];
                    clientColors[i] = col;
                    possibleColors.RemoveAt(colorIndex);
                }
                // Create a new set of color data from the color array and the client id of the current client.
                // Then, broadcast that color data over the network.
                ColorData data = new ColorData() { Colors = clientColors, ConnectionID = conn.ClientId };
                InstanceFinder.ServerManager.Broadcast(conn, data);
            }
        }
    }
}