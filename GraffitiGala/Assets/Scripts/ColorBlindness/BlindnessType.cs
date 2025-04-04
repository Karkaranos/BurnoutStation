using GraffitiGala;
using GraffitiGala.ColorSwitching;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlindnessType : MonoBehaviour
{
    // Should probably rename these once we figure out the names of specific types of colorblindness we're accounting
    // for.
    public void Option1()
    {
        AudioManager.instance.PlayOneShot(FMODEventsManager.instance.ButtonClick, Vector3.zero);
        // Color channel 1 is for colorblindness type ""
        ColorDistributor.ColorChannel = 1;
        gameObject.SetActive(false);
        ExperienceManager.ReadyClient();
    }
    public void Option2()
    {
        AudioManager.instance.PlayOneShot(FMODEventsManager.instance.ButtonClick, Vector3.zero);
        // Color channel 2 is for colorblindness type ""
        ColorDistributor.ColorChannel = 2;
        gameObject.SetActive(false);
        ExperienceManager.ReadyClient();
    }
    public void Option3()
    {
        AudioManager.instance.PlayOneShot(FMODEventsManager.instance.ButtonClick, Vector3.zero);
        // Color channel 3 is for colorblindness type ""
        ColorDistributor.ColorChannel = 3;
        gameObject.SetActive(false);
        ExperienceManager.ReadyClient();
    }
}
