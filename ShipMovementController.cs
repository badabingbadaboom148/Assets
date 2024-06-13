using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
public class ShipMovementController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSmoothness = 5f;
    public float decelerationFactor = 2f;
    public float arrivalThreshold = 0.1f;

    // Line Renderer variables
    public Material lineMaterial;

    private LineRenderer lineRenderer;
    public bool isEnabled = false;
    public bool hasMoveOrder = false;
    public bool inMoveOrderMode = false;
    public Vector3 targetPosition;
    private UnitController unitController;
    private Rigidbody rb;

    public bool isMissileButtonClicked = false;
    public bool isGunButtonClicked = false;
    public RawImage missileTargetingImage;
    public RawImage localMissileTargetingImage;
    public Text localMissileReloadingWarning;
    public Text localMissileReady;
    public Slider localMissileReload;
    private Text missileTargetingRange;

    public RawImage gunTargetingImage;
    public RawImage localGunTargetingImage;
    private Text gunTargetingRange;
    private Text gunTargetMaxRange;
    private Text gunRangeWarning;

    private LineRenderer targetingLineRenderer;
    public Material targetingLineMaterial;

    public float gunMaxRange;

    private bool escapePressed = false;

    private AIControls aiControls;

    private MasterGunController masterGunController;

    public GraphicRaycaster uiRaycaster;
    public EventSystem eventSystem;

    public GameObject sfx;
    public AudioSource audioSource;
    private void Awake()
    {
        // Ensure that UnitController is not null
        unitController = transform.GetComponent<UnitController>();
        if (unitController == null)
        {
            Debug.LogError("UnitController component not found on the GameObject.", this);
        }
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        uiRaycaster = GameObject.Find("MainCanvas").GetComponent<GraphicRaycaster>();
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        // Initialize Line Renderer in Start to ensure other components are ready
        lineRenderer = gameObject.GetComponent<LineRenderer>();

        // If LineRenderer component doesn't exist, add a new one
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.material = lineMaterial;
        lineRenderer.widthMultiplier = 1f;
        lineRenderer.positionCount = 2;
        lineRenderer.sortingLayerName = "LineRenderer";
        lineRenderer.sortingOrder = 2;

        GameObject canvasObject = GameObject.Find("MainCanvas");
        localMissileTargetingImage = Instantiate(missileTargetingImage, transform.position, missileTargetingImage.transform.rotation);
        localMissileTargetingImage.transform.gameObject.SetActive(false);
        localMissileTargetingImage.transform.SetParent(canvasObject.transform);
        localMissileReloadingWarning = localMissileTargetingImage.transform.Find("reloading warning").GetComponent<Text>();
        localMissileReady = localMissileTargetingImage.transform.Find("ready").GetComponent<Text>();
        localMissileReload = localMissileTargetingImage.GetComponentInChildren<Slider>();
        missileTargetingRange = localMissileTargetingImage.transform.GetComponentInChildren<Text>();

        localGunTargetingImage = Instantiate(gunTargetingImage, transform.position, gunTargetingImage.transform.rotation);
        localGunTargetingImage.transform.gameObject.SetActive(false);
        localGunTargetingImage.transform.SetParent(canvasObject.transform);
        if (transform.GetComponent<TeamController>().imageInstance.GetComponent<EnemyTrackIcon>() != null)
        {
            transform.GetComponent<TeamController>().imageInstance.GetComponent<EnemyTrackIcon>().trackParentObject = transform.gameObject;
        }
        gunTargetingRange = localGunTargetingImage.transform.Find("target range").GetComponent<Text>();
        gunTargetMaxRange = localGunTargetingImage.transform.Find("max range").GetComponent<Text>();
        gunTargetMaxRange.text = "MAX RNG: " + gunMaxRange;
        gunRangeWarning = localGunTargetingImage.transform.Find("range warning").GetComponent<Text>();

        targetingLineRenderer = new GameObject().AddComponent<LineRenderer>();

        targetingLineRenderer.widthMultiplier = 1f;
        targetingLineRenderer.positionCount = 2;
        targetingLineRenderer.sortingLayerName = "LineRenderer";
        targetingLineRenderer.sortingOrder = 2;
        targetingLineRenderer.material = targetingLineMaterial;

        aiControls = GetComponent<AIControls>();
        targetPosition = transform.position;

        masterGunController = transform.GetComponentInParent<MasterGunController>();
        
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Check if the AIControls component is present
        if (aiControls != null)
        {
            isEnabled = true;
            hasMoveOrder = true;
            EnterMoveOrderMode();

            // Check if the ship is currently moving towards a target
            if (hasMoveOrder)
            {
                if (Vector3.Distance(transform.position, targetPosition) > arrivalThreshold)
                {
                    MoveAndRotateToTarget(targetPosition);
                    // Draw a line to the target position
                    if (!Camera.main.GetComponent<RTSCameraController>().isCinematic || transform.GetComponent<TeamController>().isFriendly)
                    {
                        DrawLineToTarget();
                    }
                    else
                    {
                        lineRenderer.enabled = false;
                    }
                }
                else
                {
                    aiControls.ChooseDestination();
                    Debug.Log("ship is within arrival threshold");
                }
            }
        }
        else
        {
            if (unitController != null && unitController.isSelected && aiControls == null)
            {
                EnterMoveOrderMode();
            }
            else if (unitController != null && !unitController.isSelected && aiControls == null)
            {
                ExitMoveOrderMode();
            }
            if (isEnabled && aiControls == null)
            {
                if (inMoveOrderMode && unitController.isSelected && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    SetTargetPositionFromMouse();
                }
            }
            if (hasMoveOrder && isEnabled && aiControls == null)
            {
                if (Vector3.Distance(transform.position, targetPosition) > arrivalThreshold && aiControls == null)
                {
                    MoveAndRotateToTarget(targetPosition);
                    if (!Camera.main.GetComponent<RTSCameraController>().isCinematic || transform.GetComponent<TeamController>().isFriendly)
                    {
                        DrawLineToTarget();
                    }
                    else
                    {
                        lineRenderer.enabled = false;
                    }
                }
                else
                {
                    Standby();
                    ExitMoveOrderMode();
                    lineRenderer.enabled = false;
                }
            }
            Targeting();
        }
    }
    private void Targeting()
    {
        if (isMissileButtonClicked)
        {
            missileTubes missileReloading = GetComponent<missileTubes>();

            Camera.main.transform.GetComponent<RTSCameraController>().escapeIsUsed = true;
            Debug.Log("check");

            // Enable the localMissileTargetingImage
            localMissileTargetingImage.transform.gameObject.SetActive(true);

            localMissileReload.value = missileReloading.timeSince / 30f;

            if (missileReloading.timeSince < missileReloading.salvoCooldown)
            {
                localMissileReloadingWarning.transform.gameObject.SetActive(true);
                localMissileReady.transform.gameObject.SetActive(false);
            }
            else
            {
                localMissileReloadingWarning.transform.gameObject.SetActive(false);
                localMissileReady.transform.gameObject.SetActive(true);
            }

            // Convert mouse position to viewport coordinates
            Vector3 cameraWorldPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);

            // Calculate target position
            Vector3 position = Camera.main.ViewportToScreenPoint(cameraWorldPosition);

            // Set the position of the localMissileTargetingImage
            RectTransform imageRectTransform = localMissileTargetingImage.GetComponent<RectTransform>();
            imageRectTransform.position = position;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
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
                if (result.gameObject.layer != LayerMask.NameToLayer("UI")) continue;
                if (result.gameObject.GetComponent<EnemyTrackIcon>() == null) continue;
                // Calculate the screen-space position of the ship
                Vector3 targetScreenPosition = Camera.main.WorldToScreenPoint(result.gameObject.GetComponent<EnemyTrackIcon>().trackParentObject.transform.position);

                // Set the z-coordinate to a constant value
                targetScreenPosition.z = 0; // Or any other desired value

                // Set the position of the image
                imageRectTransform.position = targetScreenPosition;

                // Set the position of the line renderer
                targetingLineRenderer.enabled = true;
                targetingLineRenderer.SetPosition(0, transform.position);
                targetingLineRenderer.SetPosition(1, result.gameObject.GetComponent<EnemyTrackIcon>().trackParentObject.transform.position);

                // Display the distance
                missileTargetingRange.text = "TRG RNG: =+" + Vector3.Distance(transform.position, result.gameObject.GetComponent<EnemyTrackIcon>().trackParentObject.transform.position) + "+=";
                // Break after processing the first hit result
                break;
            }
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ship")))
            {
                // Calculate the screen-space position of the ship
                Vector3 targetScreenPosition = Camera.main.WorldToScreenPoint(hit.transform.root.position);

                // Set the z-coordinate to a constant value
                targetScreenPosition.z = 0; // Or any other desired value

                // Set the position of the image
                imageRectTransform.position = targetScreenPosition;

                // Set the position of the line renderer
                targetingLineRenderer.enabled = true;
                targetingLineRenderer.SetPosition(0, transform.position);
                targetingLineRenderer.SetPosition(1, hit.transform.root.position);

                // Display the distance
                missileTargetingRange.text = "TRG RNG: =+" + Vector3.Distance(transform.position, hit.transform.root.position) + "+=";
            }
            else
            {
                targetingLineRenderer.enabled = false;
                missileTargetingRange.text = "TRG RNG: =++=";
            }
            if (Input.GetKeyDown(KeyCode.Escape) && isMissileButtonClicked)
            {
                Camera.main.transform.GetComponent<RTSCameraController>().escapeIsUsed = false;
                isMissileButtonClicked = false;
                targetingLineRenderer.enabled = false;
                missileTargetingRange.text = "TRG RNG: =++=";
                localMissileTargetingImage.transform.gameObject.SetActive(false);
                return;
            }
        }

        if (Input.GetMouseButtonUp(1) && transform.GetComponent<UnitController>().isSelected && isMissileButtonClicked)
        {
            // Convert mouse position to viewport coordinates
            Vector3 cameraWorldPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);

            // Calculate target position
            Vector3 position = Camera.main.ViewportToScreenPoint(cameraWorldPosition);
            RectTransform imageRectTransform = localMissileTargetingImage.GetComponent<RectTransform>();
            imageRectTransform.position = position;
            Vector3 mousePosition = Input.mousePosition;

            // Create a PointerEventData object
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            uiRaycaster.Raycast(pointerEventData, results);

            // Process the results
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.layer != LayerMask.NameToLayer("UI")) continue;
                if (result.gameObject.GetComponent<EnemyTrackIcon>() == null) continue;
                missileTubes missileTubes = transform.GetComponent<missileTubes>();
                if (missileTubes != null)
                {
                    missileTubes.target = result.gameObject.GetComponent<EnemyTrackIcon>().trackParentObject;
                    missileTubes.StartCoroutine(missileTubes.FireSalvo());
                    isMissileButtonClicked = false;
                    localMissileTargetingImage.transform.gameObject.SetActive(false);
                    targetingLineRenderer.enabled = false;
                    if (audioSource != null && GetComponent<UnitController>().finalizationSoundEffect != null)
                    {
                        // Set the AudioClip to play
                        audioSource.clip = transform.GetComponent<UnitController>().finalizationSoundEffect;

                        // Play the audio clip
                        audioSource.Play();
                    }
                    sfx.GetComponent<UI_SFXManager>().PlayRandomVoiceline();
                    Camera.main.transform.GetComponent<RTSCameraController>().escapeIsUsed = false;
                    return; // Exit early after setting isMissileButtonClicked to false
                }
                break;
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ship")))
            {
                missileTubes missileTubes = transform.GetComponent<missileTubes>();
                if (missileTubes != null)
                {
                    missileTubes.target = hit.transform.gameObject;
                    missileTubes.StartCoroutine(missileTubes.FireSalvo());
                    isMissileButtonClicked = false;
                    localMissileTargetingImage.transform.gameObject.SetActive(false);
                    targetingLineRenderer.enabled = false;
                    if (audioSource != null && GetComponent<UnitController>().finalizationSoundEffect != null)
                    {
                        // Set the AudioClip to play
                        audioSource.clip = transform.GetComponent<UnitController>().finalizationSoundEffect;

                        // Play the audio clip
                        audioSource.Play();
                    }
                    sfx.GetComponent<UI_SFXManager>().PlayRandomVoiceline();
                    Camera.main.transform.GetComponent<RTSCameraController>().escapeIsUsed = false;
                    return; // Exit early after setting isMissileButtonClicked to false
                }
            }
            else
            {
                isMissileButtonClicked = false;
                localMissileTargetingImage.transform.gameObject.SetActive(false);
                targetingLineRenderer.enabled = false;
            }
        }
        else
        {
            targetingLineRenderer.enabled = false;
            missileTargetingRange.text = "TRG RNG: =++=";
        }
        if (isGunButtonClicked)
        {
            Camera.main.transform.GetComponent<RTSCameraController>().escapeIsUsed = true;
            // Enable the localMissileTargetingImage
            localGunTargetingImage.transform.gameObject.SetActive(true);

            gunRangeWarning.transform.gameObject.SetActive(false);

            // Convert mouse position to viewport coordinates
            Vector3 cameraWorldPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);

            // Calculate target position
            Vector3 position = Camera.main.ViewportToScreenPoint(cameraWorldPosition);

            // Set the position of the localMissileTargetingImage
            RectTransform imageRectTransform = localGunTargetingImage.GetComponent<RectTransform>();
            imageRectTransform.position = position;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
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
                if (result.gameObject.layer != LayerMask.NameToLayer("UI")) continue;
                if (result.gameObject.GetComponent<EnemyTrackIcon>() == null) continue;
                Debug.Log("Hit UI Element: " + result.gameObject.name);
                Transform hitTransform = result.gameObject.transform;

                // Calculate the screen-space position of the ship
                Vector3 targetScreenPosition = Camera.main.WorldToScreenPoint(hitTransform.GetComponent<EnemyTrackIcon>().trackParentObject.transform.position);

                // Set the z-coordinate to a constant value
                targetScreenPosition.z = 0; // Or any other desired value

                // Set the position of the image
                imageRectTransform.position = targetScreenPosition;

                // Set the position of the line renderer
                targetingLineRenderer.enabled = true;
                targetingLineRenderer.SetPosition(0, transform.position);
                targetingLineRenderer.SetPosition(1, hitTransform.GetComponent<EnemyTrackIcon>().trackParentObject.transform.position);

                // Display the distance
                gunTargetingRange.text = "TRG RNG: =+" + Vector3.Distance(transform.position, hitTransform.GetComponent<EnemyTrackIcon>().trackParentObject.transform.position) + "+=";
                if (Vector3.Distance(transform.position, hitTransform.GetComponent<EnemyTrackIcon>().trackParentObject.transform.position) > gunMaxRange)
                {
                    gunRangeWarning.gameObject.SetActive(true);
                }
                else
                {
                    gunRangeWarning.gameObject.SetActive(false);
                }

                // Break after processing the first hit result
                break;
            }
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ship")))
            {
                // Calculate the screen-space position of the ship
                Vector3 targetScreenPosition = Camera.main.WorldToScreenPoint(hit.transform.root.position);

                // Set the z-coordinate to a constant value
                targetScreenPosition.z = 0; // Or any other desired value

                // Set the position of the image
                imageRectTransform.position = targetScreenPosition;

                // Set the position of the line renderer
                targetingLineRenderer.enabled = true;
                targetingLineRenderer.SetPosition(0, transform.position);
                targetingLineRenderer.SetPosition(1, hit.transform.root.position);

                // Display the distance
                gunTargetingRange.text = "TRG RNG: =+" + Vector3.Distance(transform.position, hit.transform.root.position) + "+=";
                if (Vector3.Distance(transform.position, hit.transform.root.position) > gunMaxRange)
                {
                    gunRangeWarning.transform.gameObject.SetActive(true);
                }
                else
                {
                    gunRangeWarning.transform.gameObject.SetActive(false);
                }
            }
            else
            {
                targetingLineRenderer.enabled = false;
                gunTargetingRange.text = "TRG RNG: =++=";
            }
            if (Input.GetKeyDown(KeyCode.Escape) && isGunButtonClicked)
            {
                if (!escapePressed)
                {
                    Camera.main.transform.GetComponent<RTSCameraController>().escapeIsUsed = false;
                    isGunButtonClicked = false;
                    targetingLineRenderer.enabled = false;
                    gunTargetingRange.text = "TRG RNG: =++=";
                    localGunTargetingImage.transform.gameObject.SetActive(false);
                    escapePressed = true;
                }
                else
                {
                    // Second escape press: pause the game
                    Camera.main.transform.GetComponent<RTSCameraController>().PauseGame();
                }
            }
        }
        if (Input.GetMouseButtonUp(1) && transform.GetComponent<UnitController>().isSelected && isGunButtonClicked)
        {
            // Convert mouse position to viewport coordinates
            Vector3 cameraWorldPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);

            // Calculate target position
            Vector3 position = Camera.main.ViewportToScreenPoint(cameraWorldPosition);
            RectTransform imageRectTransform = localGunTargetingImage.GetComponent<RectTransform>();
            imageRectTransform.position = position;
            Vector3 mousePosition = Input.mousePosition;

            // Create a PointerEventData object
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            uiRaycaster.Raycast(pointerEventData, results);

            // Process the results
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.layer != LayerMask.NameToLayer("UI")) continue;
                if (result.gameObject.GetComponent<EnemyTrackIcon>() == null) continue;
                if (masterGunController != null && isGunButtonClicked)
                {
                    // Assign the mainTarget and secondaryTarget to the hit object
                    masterGunController.SetTargets(result.gameObject.GetComponent<EnemyTrackIcon>().trackParentObject);
                    isGunButtonClicked = false;
                    localGunTargetingImage.transform.gameObject.SetActive(false);
                    targetingLineRenderer.enabled = false;
                    if (audioSource != null && GetComponent<UnitController>().finalizationSoundEffect != null)
                    {
                        // Set the AudioClip to play
                        audioSource.clip = transform.GetComponent<UnitController>().finalizationSoundEffect;

                        // Play the audio clip
                        audioSource.Play();
                    }
                    sfx.GetComponent<UI_SFXManager>().PlayRandomVoiceline();
                    Camera.main.transform.GetComponent<RTSCameraController>().escapeIsUsed = false;
                    return;
                }
                break;
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ship")) && isGunButtonClicked)
            {
                // Check if the hit object or its parent has a MasterGunController
                MasterGunController masterGunController = transform.GetComponent<MasterGunController>();
                if (masterGunController != null && isGunButtonClicked)
                {
                    // Assign the mainTarget and secondaryTarget to the hit object
                    masterGunController.SetTargets(hit.transform.gameObject);
                    isGunButtonClicked = false;
                    localGunTargetingImage.transform.gameObject.SetActive(false);
                    targetingLineRenderer.enabled = false;
                    if (audioSource != null && GetComponent<UnitController>().finalizationSoundEffect != null)
                    {
                        // Set the AudioClip to play
                        audioSource.clip = transform.GetComponent<UnitController>().finalizationSoundEffect;

                        // Play the audio clip
                        audioSource.Play();
                    }
                    sfx.GetComponent<UI_SFXManager>().PlayRandomVoiceline();
                    Camera.main.transform.GetComponent<RTSCameraController>().escapeIsUsed = false;
                    return;
                }
            }
            else
            {
                isGunButtonClicked = false;
                localGunTargetingImage.transform.gameObject.SetActive(false);
                targetingLineRenderer.enabled = false;
            }
        }
        if (!transform.GetComponent<UnitController>().isSelected)
        {
            isMissileButtonClicked = false;
            isGunButtonClicked = false;
            localMissileTargetingImage.transform.gameObject.SetActive(false);
            localGunTargetingImage.transform.gameObject.SetActive(false);
        }
        if (masterGunController.Target != null && GetComponent<TeamController>().isFriendly)
        {
            targetingLineRenderer.enabled = true;
            targetingLineRenderer.SetPosition(0, transform.position);
            targetingLineRenderer.SetPosition(1, masterGunController.Target.transform.position);
        }
        if (masterGunController.Target != null && !masterGunController.Target.GetComponent<TeamController>().isSpotted)
        {
            masterGunController.CeaseFire();

            targetingLineRenderer.enabled = false;
        }
    }
    public void OnMissileButtonClick()
    {
        isMissileButtonClicked = true;
        // Check if the audio source and sound effect clip are set
        if (audioSource != null && GetComponent<UnitController>().selectionSoundEffect != null)
        {
            // Set the AudioClip to play
            audioSource.clip = transform.GetComponent<UnitController>().selectionSoundEffect;

            // Play the audio clip
            audioSource.Play();
        }
    }
    public void OnGunButtonClick()
    {
        isGunButtonClicked = true;
        if (audioSource != null && GetComponent<UnitController>().selectionSoundEffect != null)
        {
            // Set the AudioClip to play
            audioSource.clip = transform.GetComponent<UnitController>().selectionSoundEffect;

            // Play the audio clip
            audioSource.Play();
        }
    }

    public void EnterMoveOrderMode()
    {
        inMoveOrderMode = true;
    }

    public void ExitMoveOrderMode()
    {
        inMoveOrderMode = false;
    }

    private void SetTargetPositionFromMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("ShipPlacement")))
        {
            targetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
        }
    }

    public void MoveAndRotateToTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;

        if (direction != Vector3.zero)
        {
            direction.y = 0f;
            targetPosition.y = 0f;
            Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);

            // Use Rigidbody.MoveRotation for rotation
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, toRotation, rotationSmoothness * Time.deltaTime));

            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            float deceleratedSpeed = Mathf.Lerp(0f, moveSpeed, Mathf.Clamp01(distanceToTarget / 1f) / decelerationFactor);

            // Use Rigidbody.MovePosition for movement
            rb.MovePosition(transform.position + transform.forward * deceleratedSpeed * Time.deltaTime);
        }
    }

    private void Standby()
    {
        // Standby behavior
    }
    public void SetMoveOrder(Vector3 destination)
    {
        targetPosition = destination;
        hasMoveOrder = true;
        EnterMoveOrderMode(); // Enter move order mode when a move order is given
        DisableMovement(); // Disable movement when a move order is given
    }

    public void EnableMovement()
    {
        isEnabled = true;
        hasMoveOrder = true;
        if (audioSource != null && GetComponent<UnitController>().selectionSoundEffect != null)
        {
            // Set the AudioClip to play
            audioSource.clip = transform.GetComponent<UnitController>().selectionSoundEffect;

            // Play the audio clip
            audioSource.Play();
        }
    }

    public void DisableMovement()
    {
        isEnabled = false;
    }

    public void DrawLineToTarget()
    {
        if (!Camera.main.GetComponent<RTSCameraController>().isCinematic && transform.GetComponent<TeamController>().isFriendly)
        {
            lineRenderer.SetPosition(0, new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z));
            lineRenderer.SetPosition(1, new Vector3(targetPosition.x, transform.position.y + 1.5f, targetPosition.z));
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }
}