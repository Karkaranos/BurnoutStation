/*************************************************
Brandon Koederitz
2/21/2025
2/21/2025
Controls what elements appear or are hidden for each build.
FishNet
***************************************************/
using System;
using UnityEngine;

namespace GraffitiGala
{
    public class BuildManager : MonoBehaviour
    {
        [SerializeField] private BuildType buildType;

        private static BuildType bType;
        private static bool isInitialized;

        #region Properties
        public static BuildType BuildType
        {
            get
            {
                // Lazily finds a BuildManager if BuildType has not been initialized yet.
                if (!isInitialized)
                {
                    bType = FindObjectOfType<BuildManager>().buildType;
                    isInitialized = true;
                }
                return bType;
            }
        }

        public BuildType BuildTypeRef { get => buildType;}
        #endregion

        /// <summary>
        /// Initializes the static BuildType variable on awake if it hasnt been initialized yet.
        /// </summary>
        private void Awake()
        {
            if (!isInitialized)
            {
                bType = buildType;
                isInitialized = true;
            }
        }

        /// <summary>
        /// Checks if a given allowed build contains this build's type.
        /// </summary>
        /// <param name="allowed">The allowed builds that an object can show up in.</param>
        /// <returns>Whether or not any of the allowed build types are </returns>
        public static bool CheckBuild(HiddenBuilds allowed)
        {
            if ((allowed & HiddenBuilds.Admin) == HiddenBuilds.Admin && BuildType == BuildType.Admin)
            {
                return true;
            }
            else if ((allowed & HiddenBuilds.CityDisplay) == HiddenBuilds.CityDisplay && BuildType == 
                BuildType.CityDisplay)
            {
                return true;
            }
            else if ((allowed & HiddenBuilds.TabletStation) == HiddenBuilds.TabletStation && BuildType == 
                BuildType.TabletStation)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Defines what type this current build is.
    /// </summary>
    public enum BuildType
    {
        Admin = 0,
        CityDisplay = 1 << 0,
        TabletStation = 1 << 1
    }

    [Flags]
    public enum HiddenBuilds
    {
        None = 0,
        Admin = 1 << 0,
        CityDisplay = 1 << 1,
        TabletStation = 1 << 2

    }
}
