using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shipPlacementManager : MonoBehaviour
{
    [System.Serializable]
    public struct ShipPrefabWithPreview
    {
        public GameObject shipPrefab;
        public GameObject previewPrefab;
    }

    public ShipPrefabWithPreview[] shipPrefabsWithPreviews; // Ship prefabs and corresponding preview prefabs
    private GameObject selectedShipPrefab; // The currently selected ship
    private GameObject previewShipInstance; // The instance of the preview object
    private bool placingShip = false; // Flag to determine if ship placement is allowed
    private bool rotatingPreview = false; // Flag to determine if the preview is currently rotating
    private Quaternion targetRotation; // Target rotation for the preview object

    void Update()
    {
        // Check if ship placement is allowed
        if (placingShip)
        {
            // Handle user input (e.g., mouse move)
            HandleShipPlacement();

            // Handle user input for rotation (e.g., pressing and holding R)
            HandleRotationInput();
        }
    }

    // Called when the user selects a ship from the UI menu
    public void SelectShip(int shipIndex)
    {
        Debug.Log("SelectShip called with shipIndex: " + shipIndex);

        if (shipIndex >= 0 && shipIndex < shipPrefabsWithPreviews.Length)
        {
            // Destroy the existing preview object, if any
            Destroy(previewShipInstance);

            // Instantiate a new preview object corresponding to the selected ship
            previewShipInstance = Instantiate(shipPrefabsWithPreviews[shipIndex].previewPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);

            // Set the selected ship prefab
            selectedShipPrefab = shipPrefabsWithPreviews[shipIndex].shipPrefab;
            placingShip = true; // Enable ship placement
            rotatingPreview = false; // Reset the rotating flag
        }
    }

    // Called when the user manually selects a ship (e.g., through a button click)
    public void ManualSelectShip(GameObject shipPrefab)
    {
        // Destroy the existing preview object, if any
        Destroy(previewShipInstance);

        // Instantiate a new preview object corresponding to the selected ship
        previewShipInstance = Instantiate(GetPreviewPrefabForShip(shipPrefab), new Vector3(0f, 0f, 0f), Quaternion.identity);

        // Set the selected ship prefab
        selectedShipPrefab = shipPrefab;
        placingShip = true; // Enable ship placement
        rotatingPreview = false; // Reset the rotating flag
    }

    // Called when the user deselects the ship placement button
    public void DeselectShip()
    {
        Destroy(previewShipInstance); // Destroy the preview object
        placingShip = false; // Disable ship placement
    }

    // Handle ship placement logic
    private void HandleShipPlacement()
    {
        if (Input.GetMouseButtonDown(0) && selectedShipPrefab != null) // Check if the left mouse button is clicked and selectedShipPrefab is not null
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Move the preview ship to the cursor position
                if (previewShipInstance != null)
                {
                    previewShipInstance.transform.position = new Vector3(hit.point.x, 0f, hit.point.z);
                }

                // Instantiate and place the selected ship at the clicked position
                Instantiate(selectedShipPrefab, new Vector3(hit.point.x, 0f, hit.point.z), previewShipInstance.transform.rotation);
                Destroy(previewShipInstance); // Destroy the preview after placement
                placingShip = false; // Disable ship placement after placement
            }
        }
    }


    // Handle rotation input logic
    private void HandleRotationInput()
    {
        // Rotate the preview object gradually while the key is held down
        if (previewShipInstance != null)
        {
            float rotationAmount = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? -180f : 180f;

            if (Input.GetKey(KeyCode.R))
            {
                if (!rotatingPreview)
                {
                    targetRotation = previewShipInstance.transform.rotation * Quaternion.Euler(0f, rotationAmount, 0f); // You can adjust the rotation amount
                    rotatingPreview = true;
                }

                previewShipInstance.transform.rotation = Quaternion.RotateTowards(previewShipInstance.transform.rotation, targetRotation, Mathf.Abs(rotationAmount) * Time.deltaTime); // You can adjust the rotation speed
            }
            else
            {
                rotatingPreview = false;
            }
        }
    }

    // Helper function to get the preview prefab for a given ship prefab
    private GameObject GetPreviewPrefabForShip(GameObject shipPrefab)
    {
        foreach (var pair in shipPrefabsWithPreviews)
        {
            if (pair.shipPrefab == shipPrefab)
            {
                return pair.previewPrefab;
            }
        }

        return null;
    }
}