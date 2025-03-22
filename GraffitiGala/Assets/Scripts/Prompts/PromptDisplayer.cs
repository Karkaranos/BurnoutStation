/*************************************************
Brandon Koederitz
2/23/2025
2/26/2025
Displays a prompt given by the server to all tablets.
FishNet
***************************************************/
using FishNet;
using FishNet.Object;
using FishNet.Transporting;
using TMPro;
using UnityEngine;

namespace GraffitiGala
{
    public class PromptDisplayer : MonoBehaviour
    {
        [SerializeField] private PromptList promptList;
        [SerializeField] private GameObject promptObject;
        [SerializeField] private TMP_Text promptText;


        /// <summary>
        /// Register/unregister the RecievePrompt function to a client manager broadcast.
        /// </summary>
        private void OnEnable()
        {
            InstanceFinder.ClientManager.RegisterBroadcast<PromptData>(RecievePrompt);
        }
        private void OnDisable()
        {
            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.UnregisterBroadcast<PromptData>(RecievePrompt);
            }
        }

        /// <summary>
        /// Generates a random prompt and sends it over the network.
        /// </summary>
        public void GivePrompt()
        {
            PromptData data = new PromptData() { Prompt = promptList.GetRandomPrompt() };
            //Server_ProvidePrompt(promptList.GetRandomPrompt());
            InstanceFinder.ServerManager.Broadcast(data);
        }

        ///// <summary>
        ///// Sends a prompt over the network.
        ///// </summary>
        ///// <param name="prompt">The prompt to send.</param>
        //[ServerRpc(RequireOwnership = false)]
        //private void Server_ProvidePrompt(string prompt)
        //{
        //    RecievePrompt(prompt);
        //}

        /// <summary>
        /// Updates the prompt text to display a recieved prompt.
        /// </summary>
        /// <param name="data">The prompt to display.</param>
        /// <param name="channel">Unused channel info</param>
        private void RecievePrompt(PromptData data, Channel channel)
        {
            promptObject.SetActive(true);
            // Do fancier prompt polish here.
            promptText.text = data.Prompt;
            //Debug.Log("RecievedPrompt");
        }
    }
}