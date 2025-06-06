using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target1;   // First target
    public Transform target2;   // Second target
    public Vector3 offset = new Vector3(0f, 0f, -10f);  // Camera offset from the center point
    private Vector3 velocity;
    public float smoothTime = .5f;
    public float minZoom = 70f;
    public float maxZoom = 50f;
    public float zoomLimiter = 1f;
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();

        transform.position = new Vector3(0f, 0f, -10f);

        // Directly assigning the targets
        GameObject player1 = GameObject.Find("APlayer1");
        GameObject player2 = GameObject.Find("APlayer2");

        // Debugging the GameObject.Find calls
        Debug.Log("Attempting to find APlayer1...");
        if (player1 != null)
        {
            target1 = player1.transform;
            Debug.Log("APlayer1 found and assigned to target1.");
        }
        else
        {
            Debug.LogWarning("APlayer1 not found!");
        }

        Debug.Log("Attempting to find APlayer2...");
        if (player2 != null)
        {
            target2 = player2.transform;
            Debug.Log("APlayer2 found and assigned to target2.");
        }
        else
        {
            Debug.LogWarning("APlayer2 not found!");
        }

        Debug.Log("Targets initialized.");
    }

    void LateUpdate()
    {
        if (target1 == null || target2 == null)
            return;  // If either target is missing, don't do anything

        Move();
        Zoom();
    }

    void Move()
    {
        // Calculate the center point between the two targets
        Vector3 centerPoint = (target1.position + target2.position) / 2;

        // Apply the offset to the center point
        Vector3 newPosition = centerPoint + offset;

        // Smoothly move the camera to the new position
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    float GetGreatestDistance()
    {
        // Calculate the distance between the two targets
        float distance = Vector3.Distance(target1.position, target2.position);
        return distance;
    }

    void Zoom()
    {
        // Calculate the field of view based on the distance between the two targets
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newZoom, Time.deltaTime);
    }
}
