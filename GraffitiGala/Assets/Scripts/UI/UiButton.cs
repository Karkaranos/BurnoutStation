using System.Collections;
using System.Collections.Generic;
using GraffitiGala.Drawing;
using GraffitiGala.UI;
using UnityEngine;
using UnityEngine.UI;

public class UiButton : MonoBehaviour
{
    public int index;
    private Color paint;
    public Image Image;
    [SerializeField] private HighlightTweener tweener;

    private static UiButton currentlyActiveColorButton;
    private static UiButton CurrentlyActiveColorButton
    {
        get
        {
            return currentlyActiveColorButton;
        }
        set
        {
            if (currentlyActiveColorButton != null)
            {
                currentlyActiveColorButton.OnLoseActive();
            }
            currentlyActiveColorButton = value;
            if (currentlyActiveColorButton != null)
            {
                currentlyActiveColorButton.OnBecomeActive();
            }
        }
    }

    public void paintTransfer(Color[] paintStorage)
    {
        if (index - 1 < paintStorage.Length)
        {
            paint = paintStorage[index - 1]; // if first sphere grab 1st color in the array  
            Image.color = paint; // makes the sample red
            // When we recieve colors from the server, the first button with an index of 1 is set as the active color.
            if (index == 1)
            {
                changeBrushCl();
            }

        }
    }

    public void changeBrushCl ()
    {
        NetworkBrush.CurrentColor = paint;
        CurrentlyActiveColorButton = this;
    }    

    public void ResetColor()
    {
        paint = Color.clear;
        Image.color = Color.clear;
    }

    // Configures the twener that makes the can graphic move when this button becomes the active and not active button.
    private void OnLoseActive()
    {
        tweener.OverrideSelected = false;
    }
    private void OnBecomeActive()
    {
        tweener.OverrideSelected = true;
    }
}
