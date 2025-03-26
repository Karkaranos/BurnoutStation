/*************************************************
Author:                     Cade Naylor
Creation Date:              2/13/2025
Modified Date:              3/5/2025
Summary:                    Spawns saved images onto buildings
***************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace GraffitiGala.City
{
    public class BuildingBehavior : MonoBehaviour
    {
        [field: SerializeField, Tooltip("The sprite renderer of this building.")]
        public SpriteRenderer Rend { get; private set; }
        [field: SerializeField, Tooltip("Building Width, in Unity Units")]
        public float BuildingWidth { get; private set; }
        [field: SerializeField, Tooltip("Building Height, in Unity Units")]
        public float BuildingHeight { get; private set; }
        //[SerializeField, Tooltip("All rectangles on the building that drawings can appear in")]
        //private List<DrawingArea> validAreas = new List<DrawingArea>();
        [SerializeField, Tooltip("All rectangles on the building that drawings can appear in")]
        private List<BoxCollider2D> validAreas = new List<BoxCollider2D>();
        //[SerializeField, Range(0f, 2f), Tooltip("Scale modifier for spawned Drawings")]
        //private float scaleModifier;
        //[SerializeField, Tooltip("Area between each drawing")]
        //private float bufferDistance;
        [SerializeField] private GraffitiSettings graffitiSettings;
        [SerializeField, Tooltip("Settings that determine how the graffiti visual will start and be placed on this building.")]
        private SpriteVisualizerSettings graffitiZoomSettings;

        //[SerializeField, Tooltip("The unchanged width of a spawned object, in Unity Units")]
        //private float normalWidth;
        //[SerializeField, Tooltip("The unchanged width of a spawned object, in Unity Units")]
        //private float normalHeight;


        // The top left corner of where the sprite will spawn.
        private Vector3 localSpawningPosition;
        private int currentSpawnArea;

        private bool buildingIsFull = false;

        //public Sprite testSprite;

        public bool BuildingIsFull { get => buildingIsFull; }

        public bool SpawnDrawing(Sprite spawnMe, bool displayEffects)
        {
            // Use the bounds of the sprite inherently so that the code has an accurate size of sprites.
            float width = spawnMe.bounds.size.x * graffitiSettings.ScaleModifier;
            float height = spawnMe.bounds.size.y * graffitiSettings.ScaleModifier;
            //localSpawningPosition.x += width + graffitiSettings.BufferDistance;
            //if (localSpawningPosition.x > validAreas[currentSpawnArea].size.x + validAreas[currentSpawnArea].offset.x)
            //{
            //    if (localSpawningPosition.y - (graffitiSettings.BufferDistance + height) > 
            //        validAreas[currentSpawnArea].offset.y - validAreas[currentSpawnArea].size.y)
            //    {
            //        localSpawningPosition.y -= graffitiSettings.BufferDistance + height;
            //        localSpawningPosition.x = validAreas[currentSpawnArea].offset.x;
            //    }
            //    else if (currentSpawnArea < validAreas.Count - 1)
            //    {
            //        currentSpawnArea++;
            //        //localSpawningPosition.x = validAreas[currentSpawnArea].offset.x;
            //        //localSpawningPosition.y = validAreas[currentSpawnArea].offset.y;
            //        localSpawningPosition = GetStartingCorner(validAreas[currentSpawnArea]);
            //    }
            //    else
            //    {
            //        Debug.Log("Building is full");
            //        buildingIsFull = true;
            //        return false;
            //        // Returning here was preventing the graffiti that takes up the last space on the building to be destroyed.
            //        // Because this is predicting where the next spawn point will be, we should let the graffiti spawn normally
            //        // so it isnt lost.
            //        //Destroy(g);
            //        //return;
            //    }
            //}

            // Check for a valid location to put this graffiti.  checkEndPoint represent the bottom right corner of 
            // the graffiti.
            Vector3 checkEndPoint = localSpawningPosition;
            checkEndPoint.x += width;
            checkEndPoint.y -= height;
            // If our end point is outside the bounds, we must find a new position to spawn at.
            while (!IsWithin(validAreas[currentSpawnArea], checkEndPoint))
            {
                // If we have exceeded the bounds, then we move our spawning position and check end point typewriter
                // style down one and back to the left.
                Vector3 diagonal = checkEndPoint - localSpawningPosition;
                // This line makes the assumption that all graffiti's will have the same height.  Fine for now
                // but if we add graffiti of varying sizes then this needs to be changed.
                localSpawningPosition.y -= graffitiSettings.BufferDistance + height;
                localSpawningPosition.x = GetBounds(validAreas[currentSpawnArea]).x; //validAreas[currentSpawnArea].offset.x - validAreas[currentSpawnArea].size.x;
                checkEndPoint = localSpawningPosition + diagonal;

                // Check again if we have a valid area.  If this second check fails, it means we're beyond the bottom
                // of the valid area so we must move to the next valid area.
                if (!IsWithin(validAreas[currentSpawnArea], checkEndPoint))
                {
                    // If the new theoretical Y position exceeds the bounds of this current valid area, then move to
                    // the next if able
                    if (currentSpawnArea < validAreas.Count - 1)
                    {
                        currentSpawnArea++;
                        localSpawningPosition = GetStartingCorner(validAreas[currentSpawnArea]);
                        checkEndPoint = localSpawningPosition + diagonal;
                    }
                    // If there are no more valid areas on this building, then it is full.
                    else
                    {
                        Debug.Log("Building is Full");
                        buildingIsFull = true;
                        return false;
                    }
                }
            }

            GameObject g = new GameObject();
            g.transform.parent = this.transform;

            // Spawns the graffiti ad a position so that it's top left corner aligns with the local spawning position.
            Vector3 spawnPos = localSpawningPosition;
            spawnPos.x += width / 2;
            spawnPos.y -= height / 2;
            g.transform.localPosition = spawnPos;
            // Increment the new local spawning position.
            localSpawningPosition.x += width + graffitiSettings.BufferDistance;

            g.transform.localScale = new Vector3(graffitiSettings.ScaleModifier, graffitiSettings.ScaleModifier, 1);
            // Sets this graffiti element as the one to be highlighted.
            //GraffitiHighlighter.SetHighlightedGraffiti(g.transform);
            SpriteRenderer sRend = g.AddComponent<SpriteRenderer>();
            sRend.sprite = spawnMe;
            // Makes displaying the placement and outline effects toggleable.
            if (displayEffects)
            {
                // Add an outline to newly spawned graffiti.
                g.AddComponent<GraffitiOutliner>().Initialize(graffitiSettings.OutlineMaterial, this, sRend);
                SpritePlaceVisualizer.PlaceSpriteVisual(sRend, graffitiZoomSettings);
            }
            return true;
        }

        // Switched this to Awake so that it gets initialized when the prefab is spawned.
        void Awake()
        {
            localSpawningPosition = GetStartingCorner(validAreas[0]);
        }

        /// <summary>
        /// Gets the top left corner of a box collider.
        /// </summary>
        /// <param name="validArea">The box collider to get a corner of.</param>
        /// <returns>The position of that box collider's top left corner.</returns>
        private static Vector3 GetStartingCorner(BoxCollider2D validArea)
        {
            Vector4 bounds = GetBounds(validArea);
            return new Vector3(bounds.x, bounds.w, -1);
        }

        /// <summary>
        /// Returns the positions of a box collider's edges in the format (-X, X, -Y, Y)
        /// </summary>
        /// <param name="area">The box collider to get the edges of.</param>
        /// <returns>A Vector4 containing the positions of the edges.</returns>
        private static Vector4 GetBounds(BoxCollider2D area)
        {
            return new Vector4(area.offset.x - (area.size.x / 2), area.offset.x + (area.size.x / 2), area.offset.y -
                (area.size.y / 2), area.offset.y + (area.size.y / 2));
        }

        /// <summary>
        /// Checks if a position is within a certain box collider.
        /// </summary>
        /// <param name="area">The box collider to check.</param>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the position is located inside the box collider.</returns>
        private static bool IsWithin(BoxCollider2D area, Vector2 position)
        {
            Vector4 bounds = GetBounds(area);
            bool returnVal = false;
            // Check if the position is beyond any of the bounds.
            returnVal |= position.x < bounds.x;
            returnVal |= position.x > bounds.y;
            returnVal |= position.y < bounds.z;
            returnVal |= position.y > bounds.w;
            // If any of the above conditions are true, then the position is outside the bounds of the box and
            // this function should return false.
            return !returnVal;
        }
    }

    //[System.Serializable]
    //public class DrawingArea
    //{

    //    [SerializeField, Tooltip("The upper left corner of the building area")]
    //    private Vector2 startingPosition;
    //    [SerializeField, Tooltip("The width of the building area, in Unity Units")]
    //    private float areaWidth;
    //    [SerializeField, Tooltip("The height of the building area, in Unity Units")]
    //    private float areaHeight;

    //    public Vector2 StartingPosition { get => startingPosition; set => startingPosition = value; }
    //    public float AreaWidth { get => areaWidth; set => areaWidth = value; }
    //    public float AreaHeight { get => areaHeight; set => areaHeight = value; }
    //}
}