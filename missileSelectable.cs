using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class missileSelectable : MonoBehaviour
{
    private bool isSelected;
    public void CheckForOrbiting()
    {
        if (Input.GetKeyDown(KeyCode.F) && isSelected)
        {
            Orbiting();
        }
    }

    // Orbiting logic function
    public void Orbiting()
    {
        RTSCameraController cameraController = Camera.main.GetComponent<RTSCameraController>();
        if (cameraController != null)
        {
            cameraController.orbitingObject = transform.root;
            cameraController.isOrbiting = true;
        }
    }

    // Call this function when the missile is selected
    public void SelectMissile()
    {
        isSelected = true;
        Orbiting();
    }
}