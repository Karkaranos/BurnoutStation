/*************************************************
Brandon Koederitz
1/31/2025
2/5/2025
Takes screenshots of the current camera view with transparency and 
NaughtyAttributes, FishNet
***************************************************/
using FishNet;
using NaughtyAttributes;
using System;
using UnityEngine;

namespace GraffitiGala
{
    [RequireComponent(typeof(Camera))]
    public class GraffitiPhotographer : MonoBehaviour
    {
        #region vars
        [SerializeField, Tooltip("Objects that should be excluded from saved images.")]
        private GameObject[] _hiddenObjects = new GameObject[0];
        [SerializeField, Tooltip("Saves a copy of the saved image to this applications StreamingAssets folder.  " +
            "Do not check if more than one instance of the application will be running.  For testing purposes only.")]
        private bool _saveBackup;

        private static Camera captureCamera;
        private static bool isInitialized;
        private static bool saveBackup;
        private static GameObject[] hiddenObjects;

        #endregion

        /// <summary>
        /// When this script awakes, get a reference to the camera object attached to this GameObject.
        /// </summary>
        private void Awake()
        {
            if(!isInitialized)
            {
                if(TryGetComponent(out Camera foundCam))
                {
                    captureCamera = foundCam;
                    isInitialized = true;
                    hiddenObjects = _hiddenObjects;
                    saveBackup = _saveBackup;
                }
            }
            else
            {
                Debug.LogError("Duplicate GraffitiSaver found.  Please ensure that only one exists at a time.");
            }
        }

        ///// <summary>
        ///// Has the current GraffitiSaver take a screenshot of the current graffiti on the screen and save it to
        ///// StreamingAssets with an automatic file name.
        ///// </summary>
        ///// <returns>
        ///// The file path of the saved screenshot.
        ///// </returns>
        
        //public static void ScreenshotDrawing()
        //{
        //    // If no file name is given. the default file name is used with the date time appended.
        //    string fileName = defaultFileName + DateTime.Now.Ticks;
        //    ScreenshotDrawing(fileName);
        //}

        /// <summary>
        /// Has the current GraffitiSaver take a screenshot of the current graffiti on the screen and save it to
        /// StreamingAssets with a given file name.
        /// </summary>
        [Button]
        public static void ScreenshotDrawing()
        {
            // Screen.width and height returns the wrong values when triggered by a NaughtyAttributes button.
            //ScreenshotDrawing(Screen.width, Screen.height);
            ScreenshotDrawing(captureCamera.pixelWidth, captureCamera.pixelHeight);
        }

        /// <summary>
        /// Has the current GraffitiSaver take a screenshot of the current graffiti on the screen within given 
        /// dimensions and save it to StreamingAssets with a given file name.
        /// </summary>
        /// <remarks>
        /// Credit to Erik_Harg on the Unity Forums for this solution.
        /// </remarks>
        /// <param name="width"> The width in pixels of the screenshot to capture.</param>
        /// <param name="height">The height in pixels of the screenshot to capture.</param>
        public static void ScreenshotDrawing(int width, int height)
        {
            //Debug.Log(width + " x " + height);
            // Only allow the admin, which is always the server host, to save graffiti images.
            if (!InstanceFinder.IsServerStarted)
            {
                Debug.LogError("Non-Server client attempted to save an image.  This is not allowed.");
                return;
            }
            //Debug.Log(width + " * " + height);
            Camera cam = captureCamera != null ? captureCamera : Camera.main;

            // Hides objects in the hiddenObjects array so they are not rendered in the screenshot.
            foreach(GameObject go in hiddenObjects)
            {
                if(go == null) { continue; }
                go.SetActive(false);
            }

            Texture2D screenshotTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            RenderTexture screenshotRenderTexture = new RenderTexture(width, height, 24);

            // Renders camera data to the screenshot render texture.
            RenderTexture camRenderTexture = cam.targetTexture;
            cam.targetTexture = screenshotRenderTexture;
            cam.Render();
            cam.targetTexture = camRenderTexture;

            // Sets the screenshot render texture as the active texture then read data from said texture to the
            // screenshot Texture2D.
            RenderTexture activeTexture = RenderTexture.active;
            RenderTexture.active = screenshotRenderTexture;
            screenshotTexture.ReadPixels(new Rect(0, 0, screenshotTexture.width, screenshotTexture.height), 0, 0);
            screenshotTexture.Apply();
            RenderTexture.active = activeTexture;

            // Save screenshot as .png
            byte[] fileData = screenshotTexture.EncodeToPNG();
            //File.WriteAllBytes(filePath, fileData);

            // Re-enables hidden objects.
            foreach(GameObject go in hiddenObjects)
            {
                if (go == null) { continue; }
                go.SetActive(true);
            }

            // Destroys extra texture objects involved in the screenshot process to ensure they don't persist and
            // take up memory.
            Destroy(screenshotTexture);
            Destroy(screenshotRenderTexture);

            // Broadcasts data containing the image file over the network so the application displaying the graffiti
            // on buildings can save and use the given image.
            ImageData imgData = new ImageData() {File = fileData};
            InstanceFinder.ServerManager.Broadcast(imgData);
            // Saves the recorded image to this device's streaming assets folder.
            if(saveBackup)
            {
                ImageManagement.SaveImage(imgData, "GraffitiFile_" + DateTime.Now.Ticks);
            }

            Debug.Log("Sent screenshot over the network.");
        }
    }

}