using UnityEngine;
using UnityEngine.InputSystem;

public class FollowMouse2D : MonoBehaviour
{
    public float zPosition = 10f; // Set this to the desired depth from the camera
    private bool mousePressed;

    private InputAction mousePress;
    private InputAction mousePosition;
    public PlayerInput playerInput;


    private void Awake()
    {
        mousePressed = false;
        SetupActions();

    }

    private void SetupActions()
    {
        playerInput.currentActionMap.Enable();  //Enable action map
        mousePress = playerInput.currentActionMap.FindAction("Press");
        mousePosition = playerInput.currentActionMap.FindAction("Position");
        mousePress.started += Mouse_pressed;
        mousePress.canceled += Mouse_press_canceled;

    }

    private void OnDestroy()
    {
        mousePress.started -= Mouse_pressed;
        mousePress.canceled -= Mouse_press_canceled;
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
            Ray ray = Camera.main.ScreenPointToRay(mousePosition.ReadValue<Vector2>());

            // Calculate the point on the plane at the desired z position
            Vector3 targetPosition = ray.GetPoint(zPosition); // Get the world position at the fixed z

            // Only change x and y, keep the z from the current object
            transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
        }
        
    }

}