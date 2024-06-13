using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UI_Raycaster : MonoBehaviour
{
    public Camera mainCamera;
    public GraphicRaycaster uiRaycaster;
    public EventSystem eventSystem;

    void Update()
    {
        DetectUIUnderCursor();
    }

    void DetectUIUnderCursor()
    {
        // Get the mouse position in screen space
        Vector3 mousePosition = Input.mousePosition;

        // Create a PointerEventData object
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = mousePosition;

        // Create a list to hold the results
        List<RaycastResult> results = new List<RaycastResult>();

        // Raycast the UI
        uiRaycaster.Raycast(pointerEventData, results);

        // Process the results
        foreach (RaycastResult result in results)
        {
            //Debug.Log("Hit UI Element: " + result.gameObject.name);
        }
    }
}

