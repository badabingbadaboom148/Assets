using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackFollower : MonoBehaviour
{
    public GameObject imagePrefab;
    private GameObject imageInstance;
    // Start is called before the first frame update
    void Awake()
    {
        // Check if the image prefab is assigned
        if (imagePrefab == null)
        {
            Debug.LogError("Image prefab is not assigned!");
            return;
        }

        // Get RectTransform from the image prefab
        RectTransform imageRectTransform = imagePrefab.GetComponent<RectTransform>();
        if (imageRectTransform == null)
        {
            Debug.LogError("RectTransform not found on the instantiated image!");
            return;
        }

        // Find the Canvas by name (assuming the Canvas has the name "Canvas")
        GameObject canvasObject = GameObject.Find("MainCanvas");
        if (canvasObject == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // Set the parent of the instantiated image to the Canvas
        imageInstance = Instantiate(imagePrefab, transform.position, Quaternion.identity);
        imageInstance.transform.SetParent(canvasObject.transform, false);

        // Set the initial position of the UI Image
        UpdateImagePosition(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateImagePosition(transform.position);
    }
    void OnDestroy()
    {
        // Check if ImageInstance is not null before destroying it
        if (imageInstance != null)
        {
            Destroy(imageInstance);
        }
    }
    void UpdateImagePosition(Vector3 shipWorldPosition)
    {
        Vector3 cameraWorldPosition = Camera.main.transform.position;
        Vector3 directionToShip = (shipWorldPosition - cameraWorldPosition).normalized;
        float distanceFromCamera = 1f; // Adjust this value as needed

        Vector3 targetPosition = cameraWorldPosition + directionToShip * distanceFromCamera;
        RectTransform imageRectTransform = imageInstance.GetComponent<RectTransform>();
        imageRectTransform.position = Camera.main.WorldToScreenPoint(targetPosition);
    }
}
