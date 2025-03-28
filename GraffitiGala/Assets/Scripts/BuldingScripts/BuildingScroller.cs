/*************************************************
Brandon Koederitz
2/23/2025
2/23/2025
Scrolls buildings past the camera and wraps them around.
***************************************************/
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GraffitiGala.City
{
    public class BuildingScroller : MonoBehaviour
    {
        [SerializeField, Tooltip("The main camera that will display the city block.")] private Camera cityDisplayCam;
        [Header("Settings")]
        [SerializeField, Tooltip("The amount of space between each building")]
        private float buildingMargin;
        [SerializeField]
        private float buildingScrollSpeed;
        [SerializeField, Tooltip("The relative X position that will cause buildings to wrap around if they exceed it.")]
        private float wrapAroundPosition;
        [SerializeField, Tooltip("The Y position that all buildings should have their bottom touching, regardless of height.")] 
        private float baseline;

        // Called when a building goes to the back of the line.
        public static event Action<BuildingBehavior> OnBuildingToBack;

        private readonly List<BuildingBehavior> scrollingBuildings = new();
        public BuildingBehavior TargetBuilding { get; private set; }
        private bool scrollingRight;

        /// <summary>
        /// Sets which direction the buildings are scrolling based on the sccroll speed.
        /// </summary>
        private void Awake()
        {
            scrollingRight = MathHelpers.GetSign(buildingScrollSpeed) > 0;
            ReorderPositions();
            // Temporarily set the target to the 3rd index.
            //TargetBuilding = scrollingBuildings[2];
        }

        /// <summary>
        /// Continually scroll the buildings each frame & Updates the target building.
        /// </summary>
        private void Update()
        {
            UpdateTarget();
            ScrollBuildings();
        }

        #region Building Targeting
        /// <summary>
        /// Updates the target building.  When it becomes visible, it can no longer be the target.
        /// </summary>
        private void UpdateTarget()
        {
            if (TargetBuilding == null) { return; }
            if (cityDisplayCam.CheckObjectInCamera(TargetBuilding.Rend))
            {
                int newIndex = scrollingBuildings.IndexOf(TargetBuilding) + 1;
                // If there are no other buildings behind this one, then return to avoid out of range exception.
                if (newIndex >= scrollingBuildings.Count) { return; }
                //Debug.Log(newIndex);
                SetTarget(scrollingBuildings[newIndex]);
            }
        }

        /// <summary>
        /// Sets the target building.
        /// </summary>
        /// <param name="target">The new target building.</param>
        private void SetTarget(BuildingBehavior target)
        {
            // Debug color change.
            //if (TargetBuilding != null) 
            //{
            //    TargetBuilding.Rend.color = Color.white;
            //}
            TargetBuilding = target;
            //TargetBuilding.Rend.color = Color.red;
        }

        public bool FindNewTarget()
        {
            //Debug.Log("Finding new target");
            bool ValidTargetPredicate(BuildingBehavior item)
            {
                // Ran into a problem here where buildings at the front that were technically out of camera range were
                // elidgable to be swapped in. Now check if the item has yet to arrive on camera.
                bool isArriving = scrollingRight ? item.transform.position.x < 0 : item.transform.position.x > 0;
                return !item.BuildingIsFull && !cityDisplayCam.CheckObjectInCamera(item.Rend) && isArriving;
            }

            BuildingBehavior newTarget = scrollingBuildings.Find(ValidTargetPredicate);
            if (newTarget == null)
            {
                return false;
            }

            // Swaps the position of the new and old targets in the list.

            // Store the old index of our new target building.  The current target building will be moved to this index later.
            int otherIndex = scrollingBuildings.IndexOf(newTarget);
            //Debug.Log("New Target Found at index " + otherIndex);
            // Remove the new target here.  This means that when we get the index of the target building, we'll get the correct position
            // after any shifts.
            scrollingBuildings.Remove(newTarget);
            int targetIndex = scrollingBuildings.IndexOf(TargetBuilding);
            // Insert the new target into the index of the old target.
            scrollingBuildings.Insert(targetIndex, newTarget);
            // Move the old target building to the index that the old target builidng was at.
            scrollingBuildings.Remove(TargetBuilding);
            // Decrement the other index if the target index came before it as now the correct position has shifted one to the left
            // given the target building has been removed.
            //if (targetIndex > otherIndex) { otherIndex--; }
            scrollingBuildings.Insert(otherIndex, TargetBuilding);

            // Reroders the buildings so their positions match their position in the list.
            ReorderPositions();

            SetTarget(newTarget);
            return true;
        }
        #endregion

        #region Building Scrolling
        /// <summary>
        /// Scrolls the buildings past the camera.
        /// </summary>
        private void ScrollBuildings()
        {
            List<BuildingBehavior> buildingBuffer = new List<BuildingBehavior>();
            buildingBuffer.AddRange(scrollingBuildings);
            foreach (var building in buildingBuffer)
            {
                ScrollBuilding(building);
            }
        }

        /// <summary>
        /// Scrolls a single building and wraps it around if needed.
        /// </summary>
        /// <param name="building">THe building to scroll.</param>
        private void ScrollBuilding(BuildingBehavior building)
        {
            //Vector3 buildingPos = building.transform.localPosition;
            //buildingPos.x += buildingScrollSpeed * Time.deltaTime;
            float newPos = building.transform.localPosition.x + (buildingScrollSpeed * Time.deltaTime);
            // Checks what direction the buildings are moving in.
            if (scrollingRight)
            {
                // If moving right and the building is to the right of the wrap around position...
                if (newPos > wrapAroundPosition)
                {
                    MoveToBack(building);
                    ScrollBuilding(building);
                    return;
                }
            }
            else
            {
                // If the building is moving left and is to the left of the wrap around position...
                if (newPos < wrapAroundPosition)
                {
                    MoveToBack(building);
                    ScrollBuilding(building);
                    return;
                }
            }
            MoveToPosition(building, newPos);
        }

        /// <summary>
        /// Moves a building to the end of the city block so it can scroll through again.
        /// </summary>
        /// <param name="building">The building to move to the end.</param>
        private void MoveToBack(BuildingBehavior building)
        {
            Vector3 buildingPos = building.transform.localPosition;
            // The building that is last in the list should be the backmost building.
            BuildingBehavior backBuilding = scrollingBuildings[^1];
            // Gets the x offset that this building should go to to be behind the back building and maintain the correct
            // margins.
            float backOffset = GetBuildingDistance(building, backBuilding) * (scrollingRight ? -1 : 1);
            buildingPos.x = backBuilding.transform.localPosition.x + backOffset;
            building.transform.localPosition = buildingPos;
            // Call an event to notify listeners that this building has moved to the back of the line.
            OnBuildingToBack?.Invoke(building);
            // Moves the building to the back of the scrollingBuildings list.
            scrollingBuildings.Remove(building);
            scrollingBuildings.Add(building);
        }
        #endregion

        /// <summary>
        /// Adds a new building at the index of the current target building.
        /// </summary>
        /// <param name="buildingToAdd"></param>
        public void AddNewBuildingAsTarget(BuildingBehavior buildingToAdd)
        {
            AddNewBuilding(buildingToAdd, scrollingBuildings.IndexOf(TargetBuilding));
            SetTarget(buildingToAdd);
        }

        /// <summary>
        /// Adds an existing building to the scroll. building at a specific index.
        /// </summary>
        /// <param name="buildingToAdd">The building to add.</param>
        /// <param name="index">The index that the building should be added at.</param>
        public void AddNewBuilding(BuildingBehavior buildingToAdd, int index)
        {
            // Out of bounds indicies get clamped.  This includes if IndexOf fails to find an index and returns -1.
            index = Mathf.Clamp(index, 0, scrollingBuildings.Count - 1);
            scrollingBuildings.Insert(index, buildingToAdd);
            ReorderPositions();

            if (TargetBuilding == null)
            {
                SetTarget(buildingToAdd);
            }
        }

        /// <summary>
        /// Reorders the local positions of the buildings to be in line with the order in the scrollingBuildings list.
        /// </summary>
        private void ReorderPositions()
        {
            if (scrollingBuildings.Count == 0) { return; }
            //float currentPosition = scrollingBuildings[0].transform.localPosition.x;
            // Gets the building that is pysically at the end of the building line instead of the building at element
            // 0 in case the list has shifted.
            float currentPosition = scrollingRight ? 
                scrollingBuildings.Max(item => item.transform.localPosition.x) : 
                scrollingBuildings.Min(item => item.transform.localPosition.x);
            for (int i = 0; i < scrollingBuildings.Count; i++)
            {
                MoveToPosition(scrollingBuildings[i], currentPosition);
                int nextIndex = Mathf.Clamp(i + 1, 0, scrollingBuildings.Count - 1);
                currentPosition += GetBuildingDistance(scrollingBuildings[i], scrollingBuildings[nextIndex]);
                //Debug.Log("Distance between center of building " + scrollingBuildings[i] + " and " + scrollingBuildings[nextIndex] + " is " + GetBuildingDistance(scrollingBuildings[i], scrollingBuildings[nextIndex]));
            }
        }

        /// <summary>
        /// Moves a building to a given x point on the stree block.
        /// </summary>
        /// <param name="building">The building to move.</param>
        /// <param name="x">The x position to move to.</param>
        private void MoveToPosition(BuildingBehavior building, float x)
        {
            Vector3 buildingPos = building.transform.localPosition;
            buildingPos.x = x;
            buildingPos.y = baseline + (building.BuildingHeight / 2);
            building.transform.localPosition = buildingPos;
        }

        /// <summary>
        /// Gets the spacing between two buildings.
        /// </summary>
        /// <param name="building1">The first building.</param>
        /// <param name="building2">The second building.</param>
        /// <returns>The number of Unity Units between them.</returns>
        private float GetBuildingDistance(BuildingBehavior building1, BuildingBehavior building2)
        {
            if (building1 == null || building2 == null)
            {
                return 0;
            }
            return (building1.BuildingWidth / 2) + buildingMargin + (building2.BuildingWidth / 2);
        }
    }
}