using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RTSCameraController : MonoBehaviour
{
    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public float orbitSpeed;
    public Vector2 panLimit;

    public float scrollSpeed = 20f;
    public float minY = 10f;
    public float maxY = 80f;
    public Transform orbitingObject;
    public bool isOrbiting;

    private bool isRotating = false;
    private Vector3 lastMousePosition;

    private GameObject mainCanvas;
    public GameObject escapeMenu;
    public GameObject OptionsMenu;
    public bool isCinematic = false;

    public bool escapeIsUsed = false;
    public bool escapeMenuIsEnabled = false;
    private void Start()
    {
        mainCanvas = GameObject.Find("MainCanvas");
    }
    void Update()
    {
        // Camera panning
        Vector3 pos = transform.position;

        if (Input.GetKey("w"))
        {
            pos += transform.forward * panSpeed * Time.deltaTime;
            isOrbiting = false;
        }
        if (Input.GetKey("s"))
        {
            pos -= transform.forward * panSpeed * Time.deltaTime;
            isOrbiting = false;
        }
        if (Input.GetKey("a"))
        {
            pos -= transform.right * panSpeed * Time.deltaTime;
            isOrbiting = false;
        }
        if (Input.GetKey("d"))
        {
            pos += transform.right * panSpeed * Time.deltaTime;
            isOrbiting = false;
        }

        // Limit camera position
        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

        transform.position = pos;

        // Camera zooming
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 scrollPos = transform.position;
        scrollPos.y -= scroll * scrollSpeed * 100f * Time.deltaTime;
        scrollPos.y = Mathf.Clamp(scrollPos.y, minY, maxY);
        transform.position = scrollPos;

        // Camera rotation with middle mouse button drag
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
        }

        if (isRotating)
        {
            Vector3 deltaMousePos = Input.mousePosition - lastMousePosition;
            transform.Rotate(Vector3.up, deltaMousePos.x * Time.deltaTime * orbitSpeed, Space.World);
            transform.Rotate(Vector3.left, deltaMousePos.y * Time.deltaTime * orbitSpeed, Space.Self);
            lastMousePosition = Input.mousePosition;
        }
        if (isOrbiting)
        {
            Orbiting();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isCinematic = !isCinematic; // Toggle the cinematic mode flag

            // Set the mainCanvas active state based on the cinematic mode
            if (mainCanvas != null)
            {
                mainCanvas.SetActive(!isCinematic);
            }

            // Toggle the LineRenderer enabled state for each ship
            foreach (var ship in RadarScanner.ships)
            {
                if (ship != null)
                {
                    LineRenderer lineRenderer = ship.GetComponent<LineRenderer>();
                    if (lineRenderer != null)
                    {
                        lineRenderer.enabled = !isCinematic;
                    }
                }
            }
        }
        if (!escapeIsUsed && !escapeMenuIsEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !escapeMenuIsEnabled)
            {
                PauseGame();
                Debug.Log("pause");
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape) && escapeMenuIsEnabled)
            {
                UnpauseGame();
            }
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        escapeMenuIsEnabled = true;
        escapeIsUsed = true;
        escapeMenu.SetActive(true);
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1;
        escapeMenuIsEnabled = false;
        escapeIsUsed = false;
        escapeMenu.SetActive(false);
    }
    private float accumulatedScroll = 100f;
    public void Orbiting()
    {
        if (orbitingObject != null)
        {
            float scrollSpeed = 500f;
            float scroll = -Input.GetAxis("Mouse ScrollWheel");
            accumulatedScroll += scroll;
            accumulatedScroll = Mathf.Clamp(accumulatedScroll, 0.05f, 0.75f);
            float scrollDistance = accumulatedScroll * scrollSpeed;
            // Optional: Adjust sensitivity or clamp if needed
            // float scrollSensitivity = 1.0f;
            // scrollDistance *= scrollSensitivity;
            // scrollDistance = Mathf.Clamp(scrollDistance, -500f, 500f);

            transform.position = new Vector3(Mathf.Clamp(transform.position.x, -panLimit.x, Mathf.Infinity), Mathf.Clamp(transform.position.y, minY, maxY), transform.position.z);

            // Calculate the desired position relative to the orbitingObject
            Vector3 desiredPosition = orbitingObject.position - transform.forward * scrollDistance; // Adjust the distance as needed

            // Move the camera towards the desired position
            transform.Translate(desiredPosition - transform.position, Space.World);

            // Maintain the camera's rotation (look-at) relative to the world
            transform.LookAt(orbitingObject.position);
        }
    }
    public IEnumerator Shake(float duration, float magnitude, float distance)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;
        float maxDistance = 100f;
        float modifier = 1f - Mathf.Clamp01(distance / maxDistance);
        Debug.Log(modifier);

        while (elapsed < duration)
        {
            float xOffset = Random.Range(-1f, 1f) * magnitude;
            float yOffset = Random.Range(-1f, 1f) * magnitude;
            Vector3 offset = new Vector3(xOffset, yOffset, 0);
            transform.localPosition = originalPos + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset camera position
        transform.localPosition = originalPos;
    }
    public void OptionsButton()
    {
        escapeMenu.SetActive(false);
        OptionsMenu.SetActive(true);
    }
    public void BackButton()
    {
        escapeMenu.SetActive(true);
        OptionsMenu.SetActive(false);
    }
    public void QuitButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("mainmenu");
    }
}
