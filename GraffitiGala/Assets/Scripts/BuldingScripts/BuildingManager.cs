
using NaughtyAttributes;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GraffitiGala.City
{
    public class BuildingManager : MonoBehaviour
    {
        [SerializeField] private BuildingScroller scroller;
        [Header("Building Settings")]
        [SerializeField, Tooltip("The minimum amount of buildings that should be spawned to ensure that the screen is always scrolling.")]
        private int minBuildingCount;
        //[Header("Sprite Settings")]
        //[SerializeField, Tooltip("The normalized position of graffiti sprite's pivot points.")]
        //private Vector2 pivotPoint = new Vector2(0.5f, 0.5f);
        //[SerializeField]
        //private float pixelsPerUnit = 256f;
        [SerializeField] private GraffitiSettings spriteSettings;
        [Header("Control Panel")]
        [SerializeField, Tooltip("All buildings that can be spawned. They should all be prefabs.")]
        private BuildingBehavior[] buildingPrefabs;
        [SerializeField, Tooltip("All buildings should exist here")]
        private List<BuildingBehavior> allBuildings = new List<BuildingBehavior>();

        //private int currentBuildingIndex = 0;

        // Start is called before the first frame update
        void Start()
        {
            // Creates enough buildings to meet the specified min buildings.  Done this way to ensure we always have enough
            // buildings to have a full screen at all times.
            int buildingDiff = minBuildingCount - allBuildings.Count;
            if (buildingDiff > 0 )
            {
                for (int i = 0; i < minBuildingCount; i++)
                {
                    SpawnNewBuilding();
                }
            }

            // Load all graffiti when the program begins so that A) people can see work from previous days and B)
            // In case the event crashes, we can load all existing files people have made.
            LoadAllGraffiti();
        }

        [Button]
        public void SpawnNewBuilding()
        {
            int index = Random.Range(0, buildingPrefabs.Length);
            BuildingBehavior newBuilding = Instantiate(buildingPrefabs[index], transform.position, Quaternion.identity, transform);
            allBuildings.Add(newBuilding);
            scroller.AddNewBuildingAsTarget(newBuilding);
        }

        public void SpawnGraffitiWithPlacement(string filePath)
        {
            SpawnGraffiti(filePath, true);
        }

        public void SpawnGraffiti(string filePath, bool displayEffects)
        {
            //if (!allBuildings[currentBuildingIndex].BuildingIsFull)
            //{
            //    allBuildings[currentBuildingIndex].SpawnDrawing(ImageManagement.LoadSprite(filePath, new Vector2(0.5f, 0.5f), 256f));
            //}
            //else
            //{
            //    currentBuildingIndex++;
            //    if (currentBuildingIndex > allBuildings.Count)
            //    {
            //        SpawnNewBuilding();
            //        SpawnGraffiti(filePath);
            //    }
            //}

            if (!scroller.TargetBuilding.BuildingIsFull)
            {
                // Attempt to spawn the drawing.
                //Debug.Log("Spawning image with path " + filePath);
                bool placeSuccessful = scroller.TargetBuilding.SpawnDrawing(ImageManagement.LoadSprite(filePath, spriteSettings), displayEffects);
                // Debug.Log("Spawning graffiti " + filePath + " on building " + scroller.TargetBuilding);
                // If drawing spawning fails, then we need to continue the loop.  If spawning was sucessful, we return.
                if (placeSuccessful)
                {
                    return;
                }
            }
            // If code gets to here, that means that the target building is full.

            // Attempts to have the scroller find an already existing building that isnt full.
            if (scroller.FindNewTarget())
            {
                SpawnGraffiti(filePath, displayEffects);
            }
            else
            {
                // If there are no valid buildings that arent full, then create a new one.
                SpawnNewBuilding();
                SpawnGraffiti(filePath, displayEffects);
            }
        }

        /// <summary>
        /// Loads all Graffiti files from StreamingAssets and places them on buildings.
        /// </summary>
        private void LoadAllGraffiti()
        {
            string[] paths = System.IO.Directory.GetFiles(ImageManagement.FileDirectory);
            foreach (string path in paths)
            {
                if(Path.GetExtension(path) == ImageManagement.FILE_FORMAT)
                {
                    // Slightly worried this can cuase some problems if it tries to load too many files at once.  Will
                    // likely need to find a way to buffer this.
                    // May need to spawn over 210 files at FUSE.  Will need to stress test this later.
                    SpawnGraffiti(path, false);
                }
            }
        }
        [SerializeField] private string testFileName;

        [Button]
        private void SpawnTestGraffiti()
        {
            SpawnGraffiti(ImageManagement.GetFilePath(testFileName), true);
        }
    }
}