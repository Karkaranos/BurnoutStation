using GraffitiGala;
using GraffitiGala.ColorSwitching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PopUp : MonoBehaviour
{
    [SerializeField] private GameObject nextPopUp;




    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void IsColorblind()
    {
        AudioManager.instance.PlayOneShot(FMODEventsManager.instance.ButtonClick, Vector3.zero);
        gameObject.SetActive(false);
        nextPopUp.SetActive(true);

    }
    public void NotColorblind()
    {
        AudioManager.instance.PlayOneShot(FMODEventsManager.instance.ButtonClick, Vector3.zero);
        // Set colorblindness to the default channel.
        ColorDistributor.ColorChannel = 0;
        gameObject.SetActive(false);
        ExperienceManager.ReadyClient();
    }
    
}
