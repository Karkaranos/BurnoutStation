using UnityEngine;

public class FollowMouse2D : MonoBehaviour
{
    public float zPosition = 10f; // Set this to the desired depth from the camera

    void Update()
    {
        // Create a ray from the camera to the mouse position in world space
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Calculate the point on the plane at the desired z position
        Vector3 targetPosition = ray.GetPoint(zPosition); // Get the world position at the fixed z

        // Only change x and y, keep the z from the current object
        transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
    }
}