/*************************************************
Brandon Koederitz
2/23/2025
2/23/2025
List of prompts that can be chosen to give to players.
***************************************************/
using UnityEngine;

namespace GraffitiGala
{
    [CreateAssetMenu(fileName = "PromptList", menuName = "Graffiti Gala/Prompt List")]
    public class PromptList : ScriptableObject
    {
        [SerializeField] private string[] prompts;

        /// <summary>
        /// Gets a random prompt from the prompts list.
        /// </summary>
        /// <returns>A random prompt.</returns>
        public string GetRandomPrompt()
        {
            return prompts[Random.Range(0, prompts.Length)];
        }
    }

}