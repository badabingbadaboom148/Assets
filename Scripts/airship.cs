using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class airship : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSmoothness = 5f;
    public float decelerationFactor = 2f;
    public float arrivalThreshold = 0.1f;
    public float tiltAmount = 0.5f;
    // Line Renderer variables
    public Material lineMaterial;

    public GameObject[] fins;
    public Transform airshipMovementPlane;

    private LineRenderer lineRenderer;
    public bool isEnabled = false;
    public bool hasMoveOrder = false;
    public bool inMoveOrderMode = false;
    public Vector3 targetPosition;
    private UnitController unitController;
    private Rigidbody rb;

    private airshipHealth airshipHP;
    public float altitude = 500f;
    public float maxAltitude = 750f;
    private float originalMoveSpeed;
    private float originalRotationSmoothness;

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

    private AIControls aiControls;

    private MasterGunController masterGunController;

    public GameObject sfx;
    public AudioSource audioSource;
    private void Awake()
    {
        targetPosition = transform.position;
        originalMoveSpeed = moveSpeed;
        originalRotationSmoothness = rotationSmoothness;
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
        airshipHP = GetComponent<airshipHealth>();

        lineRenderer = gameObject.AddComponent<LineRenderer>();

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
        localGunTargetingImage.transform.parent = canvasObject.transform;
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

        masterGunController = transform.GetComponentInParent<MasterGunController>();

        audioSource = GetComponent<AudioSource>();
    }
    public float scrollAmount;
    private void Update()
    {
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
            // Check for Q key
            if (Input.GetKeyDown(KeyCode.Q))
            {
                scrollAmount += 1f;
            }
            // Check for E key
            else if (Input.GetKeyDown(KeyCode.E))
            {
                scrollAmount -= 1f;
            }
            float scrollSpeed = 10f;
            if (unitController != null && unitController.isSelected)
            {
                EnterMoveOrderMode();
                airshipMovementPlane.transform.position = new Vector3(airshipMovementPlane.transform.position.x, transform.position.y + scrollAmount, airshipMovementPlane.transform.position.z);
            }
            else if (unitController != null && !unitController.isSelected)
            {
                ExitMoveOrderMode();
            }
            if (isEnabled)
            {
                if (inMoveOrderMode && unitController.isSelected && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    SetTargetPositionFromMouse(scrollSpeed);
                }
            }
            if (hasMoveOrder && isEnabled)
            {
                if (Vector3.Distance(transform.position, targetPosition) > arrivalThreshold && aiControls == null && GetComponent<TeamController>().isFriendly)
                {
                    MoveAndRotateToTarget(targetPosition);
                    DrawLineToTarget();
                }
                else
                {
                    Standby();
                    ExitMoveOrderMode();
                    lineRenderer.enabled = false;
                }
            }
        }
        ApplyFloatingPower();

        Targeting();
    }
    public void OnMissileButtonClick()
    {
        isMissileButtonClicked = true;
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
    private void ApplyFloatingPower()
    {
        Vector3 centerOfMass = rb.centerOfMass;
        Vector3 upwardForce = rb.velocity; // Invert the velocity vector to cancel out the vertical velocity
        rb.AddForceAtPosition(-upwardForce, centerOfMass, ForceMode.VelocityChange); // Apply the force as a velocity change
    }
    private void Targeting()
    {
        MasterGunController masterGunController = transform.GetComponentInParent<MasterGunController>();
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
                    return; // Exit early after setting isMissileButtonClicked to false
                }
            }
        }
        if (isGunButtonClicked)
        {
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
            if (Input.GetKey(KeyCode.Escape) && isGunButtonClicked)
            {
                isGunButtonClicked = false;
                targetingLineRenderer.enabled = false;
                gunTargetingRange.text = "TRG RNG: =++=";
                localGunTargetingImage.transform.gameObject.SetActive(false);
                return;
            }
        }
        if (Input.GetMouseButtonUp(1) && transform.GetComponent<UnitController>().isSelected && isGunButtonClicked)
        {
            Debug.Log("check 2");
            // Raycast to determine the target object on the "Ship" layer
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ship")) && isGunButtonClicked)
            {
                // Check if the hit object or its parent has a MasterGunController
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
                    return;
                }
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
    private void SetTargetPositionFromMouse(float scrollSpeed)
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Airship")))
        {
            targetPosition = new Vector3(hit.point.x, transform.position.y + scrollAmount * scrollSpeed, hit.point.z);
        }
    }

    private Quaternion previousRotation; // Remember to initialize this somewhere

    public void MoveAndRotateToTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;

        if (direction != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);

            // Use Rigidbody.MoveRotation for rotation
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, toRotation, rotationSmoothness * Time.deltaTime));

            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            float deceleratedSpeed = Mathf.Lerp(0f, moveSpeed, Mathf.Clamp01(distanceToTarget / 1f) / decelerationFactor);

            // Use Rigidbody.MovePosition for movement
            // Calculate the movement vector
            Vector3 movement = transform.forward * deceleratedSpeed * Time.deltaTime;

            float rotationChangeY = Quaternion.Angle(previousRotation, transform.rotation);

            // Determine rotation direction
            float rotationDirection = Mathf.Sign(Vector3.Cross(previousRotation * Vector3.forward, transform.rotation * Vector3.forward).y);

            // Ensure rotationChangeY stays within the range [-180, 180]
            rotationChangeY *= rotationDirection;

            // Store the current rotation for the next frame
            previousRotation = transform.rotation;

            // Calculate the tilt angle around the z-axis based on the rotation change
            float tiltAngleZ = rotationChangeY * tiltAmount;

            // Create the tilt rotation around the z-axis
            Vector3 tiltRotationZ = transform.forward * tiltAngleZ * rb.mass * rb.angularDrag;
            // Apply the tilt rotation to the airship
            rb.AddRelativeTorque(tiltRotationZ);
            // Apply the movement to the Rigidbody's position
            rb.AddForceAtPosition(movement, transform.position, ForceMode.Force);
            PropellorSpin();
            foreach (GameObject fin in fins)
            {
                // Calculate the direction from the fin to the destination
                Vector3 directionForFins = targetPosition - fin.transform.position;
                directionForFins.y = 0f; // Ignore any vertical difference

                // Calculate the target rotation angle around the y-axis
                float angle = Vector3.SignedAngle(Vector3.forward, directionForFins, Vector3.up);

                // Clamp the rotation angle to be within the range of -90 degrees to 90 degrees
                angle = Mathf.Clamp(angle, -45, 45f);

                // Create the target rotation
                Quaternion targetRotation = Quaternion.Euler(0f, -angle, 0f);

                // Apply the rotation to the fin
                fin.transform.rotation = targetRotation;
            }
            previousRotation = transform.rotation;
        }
    }

    private void ApplyLift()
    {
        Vector3 centerOfMass = rb.centerOfMass;
        Vector3 upwardForce = -rb.velocity * Physics.gravity.magnitude; // Invert the velocity vector to cancel out the vertical velocity
        rb.AddForceAtPosition(upwardForce, centerOfMass, ForceMode.VelocityChange);
    }
    private void Standby()
    {
        // Standby behavior
    }
    private int previousFunctionalPropellorsCount = 0;
    private void PropellorSpin()
    {
        if (airshipHP.propellors.Length > 0)
        {
            int functionalPropellorsCount = airshipHP.propellors.Length;

            foreach (GameObject propellor in airshipHP.propellors)
            {
                componentHealth propellorHP = propellor.GetComponent<componentHealth>();
                if (propellorHP.Health <= 0)
                {
                    functionalPropellorsCount--;
                }
                else
                {
                    // Spin propellor around the x-axis
                    propellor.transform.Rotate(Vector3.up, 5000 * 1 / 12);
                }
            }

            // Update moveSpeed only if there's a change in functional propellors count
            if (functionalPropellorsCount != previousFunctionalPropellorsCount)
            {
                float propellorProportion = (float)functionalPropellorsCount / airshipHP.propellors.Length;
                moveSpeed = originalMoveSpeed * propellorProportion;
                rotationSmoothness = originalRotationSmoothness * propellorProportion;
                previousFunctionalPropellorsCount = functionalPropellorsCount;
            }
        }
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

    private void DrawLineToTarget()
    {
        lineRenderer.SetPosition(0, new Vector3(transform.position.x, transform.position.y, transform.position.z));
        lineRenderer.SetPosition(1, new Vector3(targetPosition.x, targetPosition.y, targetPosition.z));
        lineRenderer.enabled = true;
    }
}