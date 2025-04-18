/*************************************************
Brandon Koederitz
2/5/2025
2/5/2025
Recieves images taken by a GraffitiPhotographer and saves them to a file in this client's StreamingAssets folder.
FishNet
***************************************************/

using FishNet;
using FishNet.Transporting;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace GraffitiGala
{
    public class GraffitiReciever : MonoBehaviour
    {
        #region vars
        [SerializeField, Tooltip("The base identifier name to give saved drawing files.")]
        private string defaultFileName = "GraffitiFile_";
        [SerializeField] private UnityEvent<string> OnNewImage;
        #endregion

        /// <summary>
        /// Register the SaveImage function to a client manager broadcast.
        /// </summary>
        private void OnEnable()
        {
            InstanceFinder.ClientManager.RegisterBroadcast<ImageData>(RecieveImage);
        }
        /// <summary>
        /// Register the SaveImage function from the client manager broadcast.
        /// </summary>
        private void OnDisable()
        {
            if(InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.UnregisterBroadcast<ImageData>(RecieveImage);
            }
        }

        /// <summary>
        /// Recieves a file from a server broadcast, saves that file to a folder in StreaminAssets, and notifies
        /// event listeners that a new file was created.
        /// </summary>
        /// <param name="image">The data used to save the image.</param>
        /// <param name="channel">Unused.  Needed for this function to be registered as a broadcast.</param>
        private void RecieveImage(ImageData image, Channel channel)
        {
            // Creates a new unique file name using the default file name and the current time.
            string fileName = defaultFileName + DateTime.Now.Ticks;
            //Debug.Log("Recieved image of name " + fileName);
            // Save the image to the folder in StreamingAssets.
            string filePath = ImageManagement.SaveImage(image, fileName);
            // Notify listeners that a new image has been saved.
            OnNewImage?.Invoke(filePath);
        }
    }

}