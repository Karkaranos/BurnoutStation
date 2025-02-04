/*************************************************
Brandon Koederitz
1/31/2025
2/2/2025
Saves a list of meshes to a PNG file.
NaughtyAttributes, IO
***************************************************/

using GraffitiGala.Drawing;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GraffitiGala
{
    [RequireComponent(typeof(Camera))]
    public class GraffitiSaver : MonoBehaviour
    {
        #region vars
        #region CONSTS
        private const string FILE_FORMAT = ".png";
        #endregion
        [SerializeField, Tooltip("The name of the folder storing saved drawings.")]
        private string _folderName = "Drawings";
        [SerializeField, Tooltip("The base identifier name to give saved drawing files.")]
        private string _defaultFileName = "GraffitiFile_";
        [SerializeField, Tooltip("Objects that should be excluded from saved images.")]
        private GameObject[] _hiddenObjects = new GameObject[0];

        private static Camera captureCamera;
        private static int captureNumber;
        private static bool isInitialized;
        private static string folderName;
        private static string defaultFileName;
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
                    folderName = _folderName;
                    defaultFileName = _defaultFileName;
                    hiddenObjects = _hiddenObjects;
                }
            }
            else
            {
                Debug.LogError("Duplicate GraffitiSaver found.  Please ensure that only one exists at a time.");
            }
        }

        /// <summary>
        /// Has the current GraffitiSaver take a screenshot of the current graffiti on the screen and save it to
        /// StreamingAssets with an automatic file name.
        /// </summary>
        /// <returns>
        /// The file path of the saved screenshot.
        /// </returns>
        [Button]
        public static string ScreenshotDrawing()
        {
            // If no file name is given. the default file name is used with the date time appended.
            string fileName = defaultFileName + DateTime.Now.Ticks;
            return ScreenshotDrawing(fileName);
        }

        /// <summary>
        /// Has the current GraffitiSaver take a screenshot of the current graffiti on the screen and save it to
        /// StreamingAssets with a given file name.
        /// </summary>
        /// <param name="fileName">The name of the screenshot file.</param>
        /// <returns>The path of the saved screenshot.</returns>
        public static string ScreenshotDrawing(string fileName)
        {
            return ScreenshotDrawing(fileName, Screen.width, Screen.height);
        }

        /// <summary>
        /// Has the current GraffitiSaver take a screenshot of the current graffiti on the screen within given 
        /// dimensions and save it to StreamingAssets with a given file name.
        /// </summary>
        /// <remarks>
        /// Credit to Erik_Harg on the Unity Forums for this solution.
        /// </remarks>
        /// <param name="fileName">The name of the screenshot file.</param>
        /// <param name="width"> The width in pixels of the screenshot to capture.</param>
        /// <param name="height">The height in pixels of the screenshot to capture.</param>
        /// <returns>The path of the saved screenshot.</returns>
        public static string ScreenshotDrawing(string fileName, int width, int height)
        {
            Debug.Log(width + " * " + height);

            Camera cam = captureCamera != null ? captureCamera : Camera.main;

            //Hides objects in the hiddenObjects array so they are not rendered in the screenshot.
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
            string filePath = GetFilePath(fileName, folderName, true);
            File.WriteAllBytes(filePath, fileData);

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

            Debug.Log("Saved a screenshot to " + filePath);

            return filePath;    
        }

        /// <summary>
        /// Returns the filepath for StreamingAssets given a specific file name.  Also creates relevant folders if 
        /// they do not exist.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileDirectory">The folders within StreamingAssets to store this file at.</param>
        /// <param name="createDirectory">
        /// Whether to create new folders if they do not already exist.  If this option is set to false, you will get 
        /// an error if you attempt to access a directory that does not exist.
        /// </param>
        /// <returns>The file path navigating to that file in StreamingAssets</returns>
        private static string GetFilePath(string fileName, string fileDirectory, bool createDirectory)
        {
            string path = Path.Combine(Application.streamingAssetsPath, fileDirectory);
            if(createDirectory)
            {
                Directory.CreateDirectory(path);
            }
            path = Path.Combine(path, fileName + FILE_FORMAT);
            return path;
        }
    }

}