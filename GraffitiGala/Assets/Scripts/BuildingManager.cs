using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraffitiGala
{


    public class BuildingManager : MonoBehaviour
    {
        [Header("Control Panel")]
        [SerializeField, Tooltip("All buildings that can be spawned. They should all be prefabs.")]
        private BuildingBehavior[] buildingPrefabs;
        [SerializeField, Tooltip("All buildings should exist here")]
        private List<BuildingBehavior> allBuildings = new List<BuildingBehavior>();

        private int currentBuildingIndex = 0;

        // Start is called before the first frame update
        void Start()
        {
            SpawnNewBuilding();
        }

        public void SpawnNewBuilding()
        {
            int index = Random.Range(0, buildingPrefabs.Length);
            allBuildings.Add(Instantiate(buildingPrefabs[index], Vector3.zero, Quaternion.identity, transform));
        }

        public void SpawnGraffiti(string filePath)
        {
            if(!allBuildings[currentBuildingIndex].BuildingIsFull)
            {
                allBuildings[currentBuildingIndex].SpawnDrawing(ImageManagement.LoadSprite(filePath, new Vector2(0.5f, 0.5f), 256f));
            }
            else
            {
                currentBuildingIndex++;
                if(currentBuildingIndex > allBuildings.Count)
                {
                    SpawnNewBuilding();
                    SpawnGraffiti(filePath);
                }
            }
        }

    }
}
