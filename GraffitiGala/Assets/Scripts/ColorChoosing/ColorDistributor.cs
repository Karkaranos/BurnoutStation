/*************************************************
Brandon Koederitz
2/14/2025
2/14/2025
Sends possible colors to clients across the network.
FishNet
***************************************************/
using FishNet;
using FishNet.Connection;
using FishNet.Transporting;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace GraffitiGala.ColorSwitching
{
    public class ColorDistributor : MonoBehaviour
    {
        [SerializeField] private ColorChoices colorChoices;
        [SerializeField, Tooltip("The number of colors that are available.")] private int colorNumber;
        [SerializeField, Tooltip("The builds that need colors from the server.  Should be primarily the tablet stations.")] 
        private AllowedBuilds colorRequestingBuilds;

        private static readonly List<Color> possibleColors = new();

        /// <summary>
        /// Register the RequestColors function to a client manager broadcast.
        /// </summary>
        private void OnEnable()
        {
            InstanceFinder.ServerManager.RegisterBroadcast<ColorRequest>(RequestColors);
        }
        /// <summary>
        /// Register the RequestColors function from the client manager broadcast.
        /// </summary>
        private void OnDisable()
        {
            if (InstanceFinder.ServerManager != null)
            {
                InstanceFinder.ServerManager.UnregisterBroadcast<ColorRequest>(RequestColors);
            }
        }

        /// <summary>
        /// Sends a unique set of colors to each client.
        /// </summary>
        public void CreateColorPool()
        {
            // Resets the new list of possible colors each time a new round starts.
            possibleColors.Clear();
            possibleColors.AddRange(colorChoices.Colors);
            // Get all clients connected to this server.
            //Dictionary<int, NetworkConnection> clients = InstanceFinder.ServerManager.Clients;
            //foreach (NetworkConnection conn in clients.Values)
            //{
            //    SendColorsToClient(conn);
            //}
        }

        /// <summary>
        /// Sends a set of colors from the possible colors list to a given NetworkConnection.
        /// </summary>
        /// <param name="conn">The network connection to send colors to.</param>
        private void SendColorsToClient(NetworkConnection conn)
        {
            if (conn == null) { return; }
            if (possibleColors.Count < colorNumber)
            {
                StartCoroutine(DelayColorRequest(conn));
                //Debug.LogError("Client requested colors before colors have been assigned.  This means that a request" +
                //    " occurred before the timer started.");
                return;
            }
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

        /// <summary>
        /// Delays a color request until the server has recieved colors.
        /// </summary>
        /// <param name="conn">The network connection that requested colors early.</param>
        /// <returns>Coroutine</returns>
        private IEnumerator DelayColorRequest(NetworkConnection conn)
        {
            Debug.Log("Client requested colors early.");
            while (possibleColors.Count < colorNumber)
            {
                yield return null;
            }
            Debug.Log("Reosling early color request.");
            SendColorsToClient(conn);
        }

        /// <summary>
        /// Allows clients to request a set of colors if they join late.
        /// </summary>
        /// <param name="connection">The network connection that needs colors.</param>
        /// <param name="request">The identifier for the broadcast.</param>
        /// <param name="channel">The channel of the broadcast.</param>
        private void RequestColors(NetworkConnection connection, ColorRequest request, Channel channel)
        {
            SendColorsToClient(connection);
        }

        /// <summary>
        /// Requests colors from the server for this client if it joins late.
        /// </summary>
        public void RequestColorsFromServer()
        {
            // Prevents builds that dont need colors from requesting them.
            if (!BuildManager.CheckBuild(colorRequestingBuilds)) { return; }
            InstanceFinder.ClientManager.Broadcast(new ColorRequest());
        }
    }
}