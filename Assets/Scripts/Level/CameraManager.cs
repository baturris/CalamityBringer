using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CameraManager;

public class CameraManager : MonoBehaviour
{


    public List<Transform> targets = new List<Transform>();

    Transform p1;
    Transform p2;

    

    CameraManager camManager;

   

    Camera camComp;

    public Vector3 offset;
    private Vector3 velocity;
    public float smoothTime = .5f;
    public float minZoom = 70f;
    public float maxZoom = 50f;
    public float zoomLimiter = 1f;
    private Camera cam;


    void Start()
    {
        camManager = CameraManager.GetInstance();
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Camera component not found on CameraManager GameObject.");
        }
        camComp = cam;

    }

    void LateUpdate()
    {
        if (targets.Count == 0)
            return;

        Move();
        Zoom();
    }

    void Move()
    {
        //Vector3 centerPoint = GetCenterPoint();

        //Vector3 newPosition = new Vector3(centerPoint.x, centerPoint.y, offset.z);

        //transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
   

        transform.position = new Vector3(-0.4f, -2.4f, -1f); 
                                                          
        enabled = false;
    }

    float GetGreatestDistance()
    {
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }
        return bounds.size.x;
    }
    void Zoom()
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        camComp.fieldOfView = Mathf.Lerp(camComp.fieldOfView, newZoom, Time.deltaTime);
    }
    Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
        {
            return targets[0].position;
        }
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }
        return bounds.center;
    }


    public static CameraManager instance;

    public static CameraManager GetInstance()
    {
        return instance;
    }


    void Awake()
    { 
        instance = this;
    }

}

 