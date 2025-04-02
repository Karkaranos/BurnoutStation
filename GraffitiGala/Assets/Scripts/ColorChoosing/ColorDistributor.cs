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
using System.Linq;

namespace GraffitiGala.ColorSwitching
{
    public class ColorDistributor : MonoBehaviour
    {
        [SerializeField, Tooltip("All the potential color palettes that can be pulled from.  Items from each" +
            " palette are mutually exclusive, so only one player can have the item at index 1, etc.  The color" +
            " choices object at index 0 should be the default color palette, the rest should be color blind" +
            " palettes.")] 
        private ColorChoices[] colorChoices;
        [SerializeField, Tooltip("The number of colors that are available to players.")] private int colorNumber;
        [SerializeField, Tooltip("The builds that need colors from the server.  Should be primarily the tablet stations.")] 
        private HiddenBuilds colorRequestingBuilds;

        // This determines the color palette to choose from, and is set by color blindness options.
        public static int ColorChannel { get; set; }

        private static readonly List<int> possibleColorIndicies = new();

        /// <summary>
        /// Register the RequestColors function to a client manager broadcast.
        /// </summary>
        private void OnEnable()
        {
            InstanceFinder.ServerManager.RegisterBroadcast<ColorRequest>(HandleColorRequest);
        }
        /// <summary>
        /// Register the RequestColors function from the client manager broadcast.
        /// </summary>
        private void OnDisable()
        {
            if (InstanceFinder.ServerManager != null)
            {
                InstanceFinder.ServerManager.UnregisterBroadcast<ColorRequest>(HandleColorRequest);
            }
        }

        /// <summary>
        /// Sends a unique set of colors to each client.
        /// </summary>
        public void CreateColorPool()
        {
            // Resets the new list of possible colors each time a new round starts.
            possibleColorIndicies.Clear();
            foreach (var choice in colorChoices)
            {
                for (int i = 0; i < choice.Colors.Length; i++)
                {
                    if (!possibleColorIndicies.Contains(i))
                    {
                        possibleColorIndicies.Add(i);
                    }
                }
            }
            //possibleColorIndicies.AddRange(colorChoices.Colors);
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
        private void SendColorsToClient(NetworkConnection conn, ColorRequest request)
        {
            if (conn == null) { return; }
            if (possibleColorIndicies.Count < colorNumber)
            {
                StartCoroutine(DelayColorRequest(conn, request));
                //Debug.LogError("Client requested colors before colors have been assigned.  This means that a request" +
                //    " occurred before the timer started.");
                return;
            }
            Color[] clientColors = new Color[colorNumber];
            // Get the list of color choices that this client should pull from, based on colorblindness options.
            Color[] choicesForThisRequest;
            if (colorChoices.Length > request.Channel)
            {
                choicesForThisRequest = colorChoices[request.Channel].Colors;
            }
            else
            {
                // A channel out of bounds will be set to the default palette automatically.
                choicesForThisRequest = colorChoices.FirstOrDefault().Colors;
            }
            for (int i = 0; i < clientColors.Length; i++)
            {
                // Only used indicies from the possible indicies list that are within the range of the possible
                // colors for this request.
                List<int> possibleIndicies = possibleColorIndicies.FindAll(item => item < choicesForThisRequest.Length);
                // Gets a random color from the colorChoices list and removes that color as a possible choice.
                int rand = Random.Range(0, possibleColorIndicies.Count);
                int colorIndex = possibleIndicies[rand];
                Color col = choicesForThisRequest[colorIndex];
                clientColors[i] = col;
                // Remove the index we used to get that color from the list off colors.  Each index can only be used
                // once.
                possibleColorIndicies.Remove(rand);
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
        private IEnumerator DelayColorRequest(NetworkConnection conn, ColorRequest request)
        {
            //Debug.Log("Client requested colors early.");
            while (possibleColorIndicies.Count < colorNumber)
            {
                yield return null;
            }
            //Debug.Log("Reosling early color request.");
            SendColorsToClient(conn, request);
        }

        /// <summary>
        /// Allows clients to request a set of colors if they join late.
        /// </summary>
        /// <param name="connection">The network connection that needs colors.</param>
        /// <param name="request">The identifier for the broadcast.</param>
        /// <param name="channel">The channel of the broadcast.</param>
        private void HandleColorRequest(NetworkConnection connection, ColorRequest request, Channel channel)
        {
            SendColorsToClient(connection, request);
        }

        /// <summary>
        /// Requests colors from the server for this client if it joins late.
        /// </summary>
        public void RequestColorsFromServer()
        {
            // Prevents builds that dont need colors from requesting them.
            if (!BuildManager.CheckBuild(colorRequestingBuilds)) { return; }
            InstanceFinder.ClientManager.Broadcast(new ColorRequest() { Channel = ColorChannel});
        }
    }
}