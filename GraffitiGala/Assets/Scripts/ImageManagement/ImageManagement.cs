/*************************************************
Brandon Koederitz
2/5/2025
2/5/2025
Allows for the reading and writing of png image files via StreamingAssets
IO
***************************************************/

using System.IO;
using UnityEngine;

namespace GraffitiGala
{
    public static class ImageManagement
    {
        #region vars
        #region CONSTS
        public const string FILE_FORMAT = ".png";
        private const string FOLDER_NAME = "Drawings";
        #endregion
        private static string fileDirectory;
        #endregion

        #region Properties
        public static string FileDirectory
        {
            get
            {
                if(fileDirectory == null)
                {
                    return Path.Combine(Application.streamingAssetsPath, FOLDER_NAME);
                }
                return fileDirectory;
            }
        }
        #endregion
        /// <summary>
        /// Saves a given image to a folder in StreamingAssets specified by image data.
        /// </summary>
        /// <param name="imageData">The image of the graffiti that was taken by the server.</param>
        /// <returns>The file path to the saved image.</returns>
        public static string SaveImage(ImageData imageData, string fileName)
        {
            // Saves the passed in image data to a .png file at the drawing folder in StreamingAssets.
            string filePath = GetFilePath(fileName, true);
            File.WriteAllBytes(filePath, imageData.File);
            return filePath;
        }

        /// <summary>
        /// Returns the filepath for StreamingAssets given a specific file name.  Also creates relevant folders if 
        /// they do not exist.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="createDirectory">
        /// Whether to create new folders if they do not already exist.  If this option is set to false, you will get 
        /// an error if you attempt to access a directory that does not exist.
        /// </param>
        /// <returns>The file path navigating to that file in StreamingAssets</returns>
        public static string GetFilePath(string fileName, bool createDirectory = false)
        {
            if (createDirectory)
            {
                Directory.CreateDirectory(FileDirectory);
            }
            return Path.Combine(FileDirectory, fileName + FILE_FORMAT);
        }

        /// <summary>
        /// Loads a sprite from the streamingAssets at a given file path.
        /// </summary>
        /// <param name="filePath">The file path to load the sprite from.</param>
        /// <param name="pivotPoint">The normalized position of the sprite's pivot point.</param>
        /// <param name="pixelsPerUnit">The pixel per unit to create the sprite with.</param>
        /// <returns>The loaded image as a sprite object.</returns>
        public static Sprite LoadSprite(string filePath, Vector2 pivotPoint, float pixelsPerUnit)
        {
            byte[] rawSpriteData = File.ReadAllBytes(filePath);
            // Loads the raw image data to a Texture2D.
            Texture2D spriteTexture = new Texture2D(0, 0);
            spriteTexture.LoadImage(rawSpriteData);
            // Creates a new sprite from the Texture2D.
            Sprite sprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height),
                pivotPoint, pixelsPerUnit);
            return sprite;
        }
    }

}