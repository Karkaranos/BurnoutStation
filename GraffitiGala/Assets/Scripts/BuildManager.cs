/*************************************************
Brandon Koederitz
2/21/2025
2/21/2025
Controls what elements appear or are hidden for each build.
FishNet
***************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public BuildType buildType { get; private set; }


}

public enum BuildType
{
    Admin,
    CityDisplay,
    TabletStation
}
