using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TeamController : MonoBehaviour
{
    public GameObject imagePrefab;
    public GameObject imageInstance;
    public bool isFriendly;
    public bool isSpotted;
    public bool isSelected;
    public bool isDead = false;
    public float surfaceArea;

    public float numberOfLifeboats;
    public GameObject lifeboat;

    public string[] particleSystemNames; // Set the desired ParticleSystem names

    public List<TeamController> spottedShips = new List<TeamController>();
    private RectTransform imageRectTransform;
    private Camera mainCamera;

    public float shipsSeeing;

    public float lifeboatsSpawned = 0;

    public string ShipClassName;

    public static event Action<TeamController> OnShipDestroyed;
    void Awake()
    {
        // Add the current TeamController instance to the list of ships in RadarScanner
        RadarScanner.ships.Add(this);

        // Cache the main camera
        mainCamera = Camera.main;

        // Instantiate the image prefab
        if (imagePrefab != null)
        {
            imageInstance = Instantiate(imagePrefab, Vector3.zero, Quaternion.identity);

            GameObject canvasObject = GameObject.Find("MainCanvas");
            if (canvasObject != null)
            {
                imageInstance.transform.SetParent(canvasObject.transform, false);
                imageInstance.transform.localScale = imagePrefab.transform.localScale;
            }
            else
            {
                Debug.LogError("Canvas not found!");
            }

            // Get RectTransform from the instantiated image
            imageRectTransform = imageInstance.GetComponent<RectTransform>();

            // Set the initial position of the UI Image
            UpdateImagePosition(transform.position);
        }
        if (isFriendly)
        {
            EnableRenderersAndParticles();
            UpdateImagePosition(transform.position);
            imageInstance.SetActive(true);
        }
        Button button = imageInstance.GetComponent<Button>();
        if (button != null && GetComponent<UnitController>() != null)
        {
            button.onClick.AddListener(GetComponent<UnitController>().ButtonSelect);
        }
        if (imageInstance.GetComponent<EnemyTrackIcon>() != null)
        {
            imageInstance.GetComponent<EnemyTrackIcon>().trackParentObject = transform.gameObject;
        }
    }

    void FixedUpdate()
    {
        bool isRendering = false;
        if (isSpotted && isRendering == false || isFriendly)
        {
            EnableRenderersAndParticles();
            UpdateImagePosition(transform.position);
            imageInstance.SetActive(true);
            isRendering = true;
        }
        else
        {
            DisableRenderersAndParticles();
            imageInstance.SetActive(false);
            isRendering = false;
        }
        if (spottedShips.Count >= 1 && !isFriendly)
        {
            isSpotted = true;
            isRendering = true;
        }
        else if (spottedShips.Count == 0 && !isFriendly)
        {
            isSpotted = false;
            isRendering = false;
        }
        shipsSeeing = spottedShips.Count;
    }

    void OnDestroy()
    {
        if (imageInstance != null)
        {
            Destroy(imageInstance);
        }
    }
    public void AddSpottedShip(Transform enemyShip)
    {
        spottedShips.Add(enemyShip.GetComponent<TeamController>());
    }
    public void RemoveSpottedShip(Transform enemyShip)
    {
        spottedShips.Remove(enemyShip.GetComponent<TeamController>());
    }

    private void EnableRenderersAndParticles()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }

        foreach (string particleSystemName in particleSystemNames)
        {
            ParticleSystem ps = FindParticleSystemByName(particleSystemName);
            if (ps != null)
            {
                var emission = ps.emission;
                emission.enabled = true;
            }
        }
    }

    private void DisableRenderersAndParticles()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        foreach (string particleSystemName in particleSystemNames)
        {
            ParticleSystem ps = FindParticleSystemByName(particleSystemName);
            if (ps != null)
            {
                var emission = ps.emission;
                emission.enabled = false;
            }
        }
    }

    private ParticleSystem FindParticleSystemByName(string name)
    {
        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps.gameObject.name == name)
            {
                return ps;
            }
        }

        return null;
    }

    private void UpdateImagePosition(Vector3 shipWorldPosition)
    {
        Vector3 cameraWorldPosition = mainCamera.transform.position;
        Vector3 directionToShip = (shipWorldPosition - cameraWorldPosition).normalized;
        float distanceFromCamera = 1f; // Adjust this value as needed

        Vector3 targetPosition = cameraWorldPosition + directionToShip * distanceFromCamera;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);

        // Check if the ship is behind the camera
        if (screenPosition.z < 0)
        {
            // Place the image off-screen if the ship is behind the camera
            screenPosition = new Vector3(-1000, -1000, 0);
        }
        else
        {
            // Clamp the screen position to the screen bounds
            screenPosition.x = Mathf.Clamp(screenPosition.x, 0, Screen.width);
            screenPosition.y = Mathf.Clamp(screenPosition.y, 0, Screen.height);
        }

        imageRectTransform.position = screenPosition;
    }
    public void KilledShip()
    {
        bool isDead = true;
        OnShipDestroyed(this);
        if (isDead)
        {
            TeamController teamController = GetComponent<TeamController>();
            float surfaceArea = teamController.surfaceArea;
            if (lifeboatsSpawned < numberOfLifeboats)
            {
                for (int i = 0; i < numberOfLifeboats; i++)
                {
                    // Calculate a random position on the circle around the ship
                    Vector3 circlePosition = UnityEngine.Random.onUnitSphere * (surfaceArea / 10) + transform.position;
                    circlePosition = new Vector3(circlePosition.x, 0, circlePosition.z);

                    // Instantiate the lifeboat prefab at the calculated position
                    GameObject newLifeboat = Instantiate(lifeboat, circlePosition, Quaternion.identity);

                    // Calculate the direction vector pointing from the ship to the lifeboat
                    Vector3 directionToLifeboat = (circlePosition - transform.position).normalized;

                    // Calculate the rotation needed to point the lifeboat away from the ship
                    Quaternion lifeboatRotation = Quaternion.LookRotation(-directionToLifeboat, Vector3.up);

                    // Apply the rotation to the lifeboat
                    newLifeboat.transform.rotation = lifeboatRotation;

                    lifeboatsSpawned++;
                }
            }
        }
    }
}