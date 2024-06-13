using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UnitController : MonoBehaviour
{
    private Renderer[] renderers;
    private Color[] originalColors;
    private ShipMovementController shipMovementController;

    public GameObject controlMenuPrefab; // Reference to your control menu prefab
    public Button moveButton; // Reference to your move button
    public Button attackButton;
    public bool isSelected = false;
    private RTSCameraController cameraController;

    public AudioClip selectionSoundEffect;
    public AudioClip finalizationSoundEffect;
    public AudioSource audioSource;

    public float numberOfTimesEscapePressed = 0;
    void Awake()
    {
        // Get all renderers in the GameObject and its children
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        // Store the original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }

        // Get the ShipMovementController script attached to this GameObject
        shipMovementController = GetComponent<ShipMovementController>();
        cameraController = Camera.main.GetComponent<RTSCameraController>();

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (isSelected)
        {
            if (cameraController != null)
            {
                if (Input.GetKey(KeyCode.F))
                {
                    cameraController.orbitingObject = transform;
                    cameraController.isOrbiting = true;
                    cameraController.Orbiting();
                }
            }
            if (numberOfTimesEscapePressed == 0)
            {
                cameraController.escapeIsUsed = true;
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    isSelected = false;
                    Deselect();
                    numberOfTimesEscapePressed++;
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    numberOfTimesEscapePressed = 0;
                    cameraController.escapeIsUsed = false;
                }
            }
        }
        else
        {
            numberOfTimesEscapePressed = 0;
            cameraController.escapeIsUsed = false;
        }
    }
    public void ButtonSelect()
    {
        // Check if this ship is not currently selected
        if (!isSelected)
        {

            // Iterate through all ships to check if any other ship is selected
            foreach (var ship in RadarScanner.ships)
            {
                // Skip this ship
                if (ship == GetComponent<TeamController>() || ship.transform.GetComponent<RadarScanner>() == null)
                    continue;

                // Check if another ship is selected
                if (ship.GetComponent<UnitController>().isSelected)
                {
                    // Deselect the currently selected ship
                    ship.GetComponent<UnitController>().Deselect();

                    // Select this ship
                    Select();
                    return;
                }
            }

            // If no other ship is selected, simply select this ship
            Select();
        }
        else
        {
            // Deselect the ship if it's already selected
            Deselect();
        }
        if (audioSource != null && selectionSoundEffect != null)
        {
            // Set the AudioClip to play
            audioSource.clip = selectionSoundEffect;

            // Play the audio clip
            audioSource.Play();
        }
    }

    void OnMouseEnter()
    {
        if (!isSelected)
        {
            // Change the material color on hover for the GameObject and its children
            ChangeMaterialColor(Color.yellow);
        }
    }

    void OnMouseExit()
    {
        if (!isSelected)
        {
            // Restore the original material color for the GameObject and its children
            ChangeMaterialColor(originalColors);
        }
    }

    void OnMouseDown()
    {
        // Check if this ship is not currently selected
        if (!isSelected)
        {

            // Iterate through all ships to check if any other ship is selected
            foreach (var ship in RadarScanner.ships)
            {
                // Skip this ship
                if (ship == this || ship.transform.GetComponent<RadarScanner>() == null)
                    continue;

                // Check if another ship is selected
                if (ship.GetComponent<UnitController>().isSelected)
                {
                    // Deselect the currently selected ship
                    ship.GetComponent<UnitController>().Deselect();

                    // Select this ship
                    Select();
                    return;
                }
            }

            // If no other ship is selected, simply select this ship
            Select();
        }
        else
        {
            // Deselect the ship if it's already selected
            Deselect();
        }
    }

    // Helper method to change material color for the GameObject and its children
    private void ChangeMaterialColor(Color color)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                renderers[i].material.color = color;
            }
        }
    }

    // Helper method to restore original material color for the GameObject and its children
    private void ChangeMaterialColor(Color[] colors)
    {
        for (int i = 0; i < renderers.Length && i < colors.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                renderers[i].material.color = colors[i];
            }
        }
    }

    // Method to handle unit selection
    public void Select()
    {
        isSelected = true;
        TeamController teamController = transform.GetComponent<TeamController>();

        if (controlMenuPrefab != null && teamController.isFriendly)
        {
            controlMenuPrefab.SetActive(true);
        }
    }

    // Method to handle unit deselection
    public void Deselect()
    {
        isSelected = false;
        TeamController teamController = transform.GetComponent<TeamController>();

        if (controlMenuPrefab != null && teamController.isFriendly)
        {
            controlMenuPrefab.SetActive(false);
        }
        // Restore the original material color when the unit is deselected
        ChangeMaterialColor(originalColors);
    }

    // Method to handle move button click
    public void OnMoveButtonClick()
    {
        Debug.Log("Move button clicked!");

        if (isSelected && shipMovementController != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Set the move order to the mouse position on the ground
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Water")))
                {
                    shipMovementController.SetMoveOrder(new Vector3(hit.point.x, transform.position.y, hit.point.z));
                }
            }

            // Invoke the same action as the control menu button click
            OnControlMenuButtonClick();
        }
        // Check if the audio source and sound effect clip are set
        if (audioSource != null && selectionSoundEffect != null)
        {
            // Set the AudioClip to play
            audioSource.clip = selectionSoundEffect;

            // Play the audio clip
            audioSource.Play();
        }
    }

    // Method to handle button clicks within the control menu
    private void OnControlMenuButtonClick()
    {
        // Disable the control menu when any button is clicked
        controlMenuPrefab.SetActive(false);
    }

    bool IsPointerOverUIObject()
    {
        // Check if the mouse is over any UI element
        return EventSystem.current.IsPointerOverGameObject();
    }

    bool IsPointerOverControlMenu()
    {
        // Check if the mouse is over the control menu
        if (controlMenuPrefab != null)
        {
            RectTransform rectTransform = controlMenuPrefab.GetComponent<RectTransform>();
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition);
        }
        return false;
    }
}
