using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class FollowMouse2D : MonoBehaviour
{
    public float zPosition = 10f; // Set this to the desired depth from the camera
    private bool mousePressed;

    private InputAction mousePress;
    public PlayerInput playerInput;


    private void Start()
    {
        mousePressed = false;
        SetupActions();

    }



    private void SetupActions()
    {
        playerInput.currentActionMap.Enable();  //Enable action map
        mousePress = playerInput.currentActionMap.FindAction("Press");
        mousePress.started += Mouse_pressed;
        mousePress.canceled += Mouse_press_canceled;

    }

    private void Mouse_pressed(InputAction.CallbackContext obj)
    {
        mousePressed = true;

    }
    private void Mouse_press_canceled(InputAction.CallbackContext obj)
    {
        mousePressed = false;

    }

    private void FixedUpdate()
    {
        if (mousePressed)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Calculate the point on the plane at the desired z position
            Vector3 targetPosition = ray.GetPoint(zPosition); // Get the world position at the fixed z

            // Only change x and y, keep the z from the current object
            transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
        }
        
    }

}