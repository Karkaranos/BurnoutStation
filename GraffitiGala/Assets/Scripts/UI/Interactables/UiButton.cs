using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraffitiGala;
using GraffitiGala.Drawing;
using GraffitiGala.UI;
using UnityEngine;
using UnityEngine.UI;

public class UiButton : MonoBehaviour
{
    public int index;
    private Color paint;
    [SerializeField] private Image[] coloredImages;
    [SerializeField] private HighlightAnimator highlightAnim;
    [SerializeField, Tooltip("During what state is this button be interactable.")]
    private ExperienceState[] stateMask;

    private bool startFix = false;

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
            SetImageColor(paint);
            // When we recieve colors from the server, the first button with an index of 1 is set as the active color.
            if (index == 1)
            {
                changeBrushCl();
            }

        }
    }

    public void changeBrushCl ()
    {
        if (stateMask.Contains(ExperienceManager.GetState()))
        {
            NetworkBrush.CurrentColor = paint;
            CurrentlyActiveColorButton = this;
            if (FindObjectOfType<BuildManager>().BuildTypeRef == BuildType.TabletStation)
            {
                if(!startFix)
                {
                    startFix = true;
                }
                else
                {
                    AudioManager.instance.PlayOneShot(FMODEventsManager.instance.SwitchCans, Vector3.zero);
                }

            }
        }
    }    

    public void ResetColor()
    {
        paint = Color.white;
        SetImageColor(Color.white);

        CurrentlyActiveColorButton = null;
    }

    /// <summary>
    /// Changes the color of all images whose color should update to match this button's color.
    /// </summary>
    /// <param name="color">The color to set for the images.</param>
    private void SetImageColor(Color color)
    {
        foreach (Image img in coloredImages)
        {
            img.color = color; // makes the sample red
        }
    }

    // Configures the twener that makes the can graphic move when this button becomes the active and not active button.
    private void OnLoseActive()
    {
        highlightAnim.OverrideSelected = false;
    }
    private void OnBecomeActive()
    {
        highlightAnim.OverrideSelected = true;
    }
}
