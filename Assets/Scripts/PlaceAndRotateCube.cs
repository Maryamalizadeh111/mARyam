using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlaneNewInputSystem : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject placedPrefab;

    [SerializeField]
    [Tooltip("Height above the detected plane (in meters).")]
    float heightAboveFloor = 1.5f;

    GameObject spawnedObject;
    TouchControls controls;
    bool isPressed;
    ARRaycastManager aRRaycastManager;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // Colors for random selection
    private readonly Color[] cubeColors = new Color[] {
        new Color32(207, 89, 64, 255),  // #CF5940
        new Color32(26, 243, 72, 255),  // #1AF348
        new Color32(110, 64, 207, 255)  // #6E40CF
    };

    // Flag to check if the cube is rotating
    private bool isRotating = false;

    private void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        controls = new TouchControls();

        controls.control.touch.performed += _ => isPressed = true;
        controls.control.touch.canceled += _ => isPressed = false;
    }

    void Update()
    {
        // Check if there is any pointer device connected to the system.
        // Or if there is existing touch input.
        if (Pointer.current == null || !isPressed)
            return;
        
        // Store the current touch position.
        var touchPosition = Pointer.current.position.ReadValue();

        // Check if we hit a detected AR plane
        if (aRRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;

            // If no cube has been spawned yet, spawn the cube 1.5 meters above the plane
            if (spawnedObject == null)
            {
                Vector3 spawnPosition = hitPose.position + Vector3.up * heightAboveFloor;
                spawnedObject = Instantiate(placedPrefab, spawnPosition, hitPose.rotation);
            }
            else if (!isRotating) // Only rotate and change color if the cube is not currently rotating
            {
                StartCoroutine(RotateCubeOverTime());
                ChangeCubeColor(GetRandomColor());
            }
        }
    }

    // Coroutine to smoothly rotate the cube around the Y axis
    IEnumerator RotateCubeOverTime()
    {
        isRotating = true; // Set the rotating flag

        float totalRotation = 0f;
        float rotationStep = 5f;  // Degrees per step
        float timeBetweenSteps = 0.03f;  // Pause duration between steps

        while (totalRotation < 360f)
        {
            spawnedObject.transform.Rotate(Vector3.up, rotationStep);
            totalRotation += rotationStep;
            yield return new WaitForSeconds(timeBetweenSteps);
        }

        // Ensure the cube ends with a clean rotation of 360 degrees
        spawnedObject.transform.rotation = Quaternion.Euler(0f, Mathf.Round(spawnedObject.transform.eulerAngles.y), 0f);

        isRotating = false; // Reset the rotating flag after finishing the rotation
    }

    // Function to change the color of the cube
    void ChangeCubeColor(Color newColor)
    {
        Renderer cubeRenderer = spawnedObject.GetComponent<Renderer>();
        if (cubeRenderer != null)
        {
            cubeRenderer.material.color = newColor;
        }
    }

    // Function to get a random color different from the current one
    Color GetRandomColor()
    {
        Color newColor = cubeColors[Random.Range(0, cubeColors.Length)];
        return newColor;
    }

    private void OnEnable()
    {
        controls.control.Enable();
    }

    private void OnDisable()
    {
        controls.control.Disable();
    }
}
