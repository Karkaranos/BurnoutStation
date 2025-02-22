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

        
        private static bool isInitialized;

        private void Awake()
        {
            
        }
    }

    /// <summary>
    /// Defines what type this current build is.
    /// </summary>
    public enum BuildType
    {
        Admin,
        CityDisplay,
        TabletStation
    }

    [Flags]
    public enum AllowedBuilds
    {
        Admin = 0,
        CityDisplay = 1 << 0,
        TabletStation = 1 << 1

    }
}
