using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class tutorialManager : MonoBehaviour
{
    private TutorialActionMap actionMap;

    public event Action<InputAction.CallbackContext> movement;
    public event Action<InputAction.CallbackContext> MouseRightHold;
    public event Action<InputAction.CallbackContext> Mouse;
    public event Action<InputAction.CallbackContext> AirshipAltitude;
    public event Action<InputAction.CallbackContext> Focus;
    public event Action<InputAction.CallbackContext> CheckHealth;
    // Start is called before the first frame update
    void Awake()
    {
        actionMap = new();
    }
    private void OnEnable()
    {
        actionMap.Enable();
    }
    private void OnDisable()
    {
        actionMap.Disable();
    }

    private void onMove(InputAction.CallbackContext ctx)
    {

    }
    private void OnMouseDrag(InputAction.CallbackContext ctx)
    {

    }
    private void MouseRelease(InputAction.CallbackContext ctx)
    {

    }
    private void changeAltitiude(InputAction.CallbackContext ctx)
    {

    }
    public void FireCannon(InputAction.CallbackContext ctx)
    {

    }
    public void checkHealth(InputAction.CallbackContext ctx)
    {

    }
}