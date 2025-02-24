/*************************************************
Brandon Koederitz
2/23/2025
2/23/2025
List of prompts that can be chosen to give to players.
FishNet
***************************************************/
using FishNet.Object;
using TMPro;
using UnityEngine;

namespace GraffitiGala
{
    public class PromptDisplayer : NetworkBehaviour
    {
        [SerializeField] private PromptList promptList;
        [SerializeField] private TMP_Text promptText;

        /// <summary>
        /// Generates a random prompt and sends it over the network.
        /// </summary>
        public void GivePrompt()
        {
            Server_ProvidePrompt(promptList.GetRandomPrompt());
        }

        /// <summary>
        /// Sends a prompt over the network.
        /// </summary>
        /// <param name="prompt">The prompt to send.</param>
        [ServerRpc(RequireOwnership = false)]
        private void Server_ProvidePrompt(string prompt)
        {
            Client_RecievePrompt(prompt);
        }

        /// <summary>
        /// Updates the prompt text to display a recieved prompt.
        /// </summary>
        /// <param name="prompt">The prompt to display.</param>
        [ObserversRpc]
        private void Client_RecievePrompt(string prompt)
        {
            promptText.gameObject.SetActive(true);
            // Do fancier prompt polish here.
            promptText.text = prompt;
            //Debug.Log("RecievedPrompt");
        }
    }
}