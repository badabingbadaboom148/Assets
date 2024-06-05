using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectWithMenu : MonoBehaviour
{
    public GameObject menuPrefab;
    private GameObject menuInstance;

    void Start()
    {
        // Instantiate a new menu for this object at the start of the frame
        InstantiateMenu();
    }

    // Called when the button is clicked
    public void ToggleMenu()
    {
        if (menuInstance != null)
        {
            // Toggle the menu's visibility
            menuInstance.SetActive(!menuInstance.activeSelf);

            if (menuInstance.activeSelf)
            {
                // Menu was opened, add the click outside handler
                AddClickOutsideHandler();
            }
            else
            {
                // Menu was closed, remove the click outside handler
                RemoveClickOutsideHandler();
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

    private void InstantiateMenu()
    {
        // Instantiate a new menu for this object
        menuInstance = Instantiate(menuPrefab, transform.position, Quaternion.identity);
        // Disable the menu
        menuInstance.SetActive(false);

        // Pass information to the menu or perform any initialization here
        //menuInstance.GetComponent<CommandMenuController>().SetAssociatedObject(this.gameObject);
    }
}
