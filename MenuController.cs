using UnityEngine;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour
{
    public GameObject menuPrefab;
    private GameObject menuInstance;

    // Called when the button is clicked
    public void ToggleMenu()
    {
        if (menuInstance == null)
        {
            // Instantiate the menu if it doesn't exist
            menuInstance = Instantiate(menuPrefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            // Toggle the menu's visibility
            menuInstance.SetActive(!menuInstance.activeSelf);

            if (!menuInstance.activeSelf)
            {
                // Menu was closed, remove the click outside handler
                RemoveClickOutsideHandler();
            }
            else
            {
                // Menu was opened, add the click outside handler
                AddClickOutsideHandler();

                // Adjust the menu position based on ship visibility
                AdjustMenuPosition();
            }
        }
    }

    // Called when the button is unselected or a click occurs outside the menu
    public void CloseMenu()
    {
        if (menuInstance != null)
        {
            // Close the menu
            menuInstance.SetActive(false);

            // Remove the click outside handler
            RemoveClickOutsideHandler();
        }
    }

    private void AddClickOutsideHandler()
    {
        // Add a listener to detect clicks outside the menu
        if (!EventSystem.current.alreadySelecting)
        {
            EventSystem.current.SetSelectedGameObject(menuInstance, null);
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void RemoveClickOutsideHandler()
    {
        // Remove the listener to stop detecting clicks outside the menu
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void AdjustMenuPosition()
    {
        if (menuInstance != null)
        {
            // Get the screen width and height
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Get the menu's RectTransform component
            RectTransform menuRectTransform = menuInstance.GetComponent<RectTransform>();

            // Set the initial position to the center of the screen
            Vector3 menuPosition = new Vector3(screenWidth / 2f, screenHeight / 2f, 0f);

            // If the ship is not within view, position the menu near the screen edges
            if (!IsShipVisible())
            {
                float edgeDistance = Mathf.Min(screenWidth, screenHeight) * 0.4f; // Adjust this value as needed

                // Calculate the direction to the center of the screen
                Vector3 directionToCenter = (menuPosition - menuRectTransform.position).normalized;

                // Adjust the menu position near the screen edges
                menuPosition = menuPosition + directionToCenter * edgeDistance;
            }

            // Set the adjusted position for the menu
            menuRectTransform.position = menuPosition;
        }
    }

    private bool IsShipVisible()
    {
        // Check if the ship is within the camera's field of view
        // (Assuming the ship is part of the scene and not in a separate script)
        Renderer shipRenderer = GetComponent<Renderer>();
        if (shipRenderer != null)
        {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(shipRenderer.bounds.center);
            return screenPoint.z > 0;
        }

        return false;
    }
}
