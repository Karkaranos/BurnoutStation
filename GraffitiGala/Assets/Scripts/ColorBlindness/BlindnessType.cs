using GraffitiGala;
using GraffitiGala.ColorSwitching;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlindnessType : MonoBehaviour
{
    [SerializeField] private GameObject waitingForPlayersText;
    // Should probably rename these once we figure out the names of specific types of colorblindness we're accounting
    // for.
    public void SetColorblindness(int channel)
    {
        AudioManager.instance.PlayOneShot(FMODEventsManager.instance.ButtonClick, Vector3.zero);
        // Color channel 1 is for colorblindness type ""
        ColorDistributor.ColorChannel = channel;
        gameObject.SetActive(false);
        ExperienceManager.ReadyClient();
        waitingForPlayersText.SetActive(true);
    }
    //public void Option2()
    //{
    //    AudioManager.instance.PlayOneShot(FMODEventsManager.instance.ButtonClick, Vector3.zero);
    //    // Color channel 2 is for colorblindness type ""
    //    ColorDistributor.ColorChannel = 2;
    //    gameObject.SetActive(false);
    //    ExperienceManager.ReadyClient();
    //}
    //public void Option3()
    //{
    //    AudioManager.instance.PlayOneShot(FMODEventsManager.instance.ButtonClick, Vector3.zero);
    //    // Color channel 3 is for colorblindness type ""
    //    ColorDistributor.ColorChannel = 3;
    //    gameObject.SetActive(false);
    //    ExperienceManager.ReadyClient();
    //}
}
