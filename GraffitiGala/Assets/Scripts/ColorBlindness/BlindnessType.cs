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
        // Color channel 1 is for colorblindness type ""
        ColorDistributor.ColorChannel = 1;
        gameObject.SetActive(false);

    }
    public void Option2()
    {
        // Color channel 2 is for colorblindness type ""
        ColorDistributor.ColorChannel = 2;
        gameObject.SetActive(false);
    }
    public void Option3()
    {
        // Color channel 3 is for colorblindness type ""
        ColorDistributor.ColorChannel = 3;
        gameObject.SetActive(false);

    }
}
