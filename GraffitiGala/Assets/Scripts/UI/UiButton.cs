using System.Collections;
using System.Collections.Generic;
using GraffitiGala.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class UiButton : MonoBehaviour
{
    public int index;
    private Color paint;
    public Image Image;

    public void paintTransfer(Color[] paintStorage)
    {
        if (index - 1 < paintStorage.Length)
        {
            paint = paintStorage[index - 1]; // if first sphere grab 1st color in the array  
            Image.color = paint; // makes the sample red

        }
    }

    public void changeBrushCl ()
    {
        NetworkBrush.CurrentColor = paint;

    }    

    public void ResetColor()
    {
        paint = Color.clear;
        Image.color = Color.clear;
    }
}
