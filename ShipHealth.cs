using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipHealth : MonoBehaviour
{
    public GameObject[] command;
    public GameObject[] boilers;
    public GameObject rudder;
    private bool rudderAlive = false;
    public GameObject[] guns;
    public GameObject magazine;
    public GameObject hull;
    public GameObject[] ballasts;

    public float armorThickness;
    public float internalDensity;

    private float floatingPower;
    private float decreaseRate = 5f;
    private Buoyancy buoyancy;
    private Vector3 randomTorque;
    componentHealth magazineHealth;
    componentHealth hullHealth;
    public GameObject magDetVFX;
    public ParticleSystem explosionParticlePrefab;
    private bool isExploded = false;
    public float explosionRadius = 200f;
    private Rigidbody rb;

    private ShipMovementController shipMovementController;

    public GameObject healthCube;
    private List<GameObject> healthCubes = new List<GameObject>();
    public bool healthCubesAreShowing;

    private UnitController unitController;

    public GameObject floatingTextPrefab;
    private Text textComponent;
    private void Start()
    {
        buoyancy = transform.GetComponent<Buoyancy>();
        magazineHealth = magazine.GetComponent<componentHealth>();
        hullHealth = hull.GetComponent<componentHealth>();
        rb = transform.GetComponent<Rigidbody>();
        SpawnHealthCubes(command);
        SpawnHealthCubes(boilers);
        SpawnHealthCubes(guns);
        SpawnHealthCubes(new GameObject[] { magazine, hull, rudder});

        unitController = transform.GetComponent<UnitController>();

        shipMovementController = transform.GetComponent<ShipMovementController>();

        floatingTextPrefab.SetActive(false);
        textComponent = floatingTextPrefab.transform.GetComponent<Text>();
    }
    public void checkForDamage()
    {
        bool anyCommandAlive = false;

        // Check if any command has health greater than zero
        foreach (GameObject cmd in command)
        {
            if (cmd != null)
            {
                componentHealth commandHealth = cmd.GetComponent<componentHealth>();
                if (commandHealth != null && commandHealth.Health > 0)
                {
                    anyCommandAlive = true;
                    break; // No need to check further once a living command is found
                }
            }
        }

        // Enable or disable unit and ship movement controllers based on command status
        bool shouldEnableControllers = anyCommandAlive && hullHealth.Health > 0;
        transform.GetComponent<UnitController>().enabled = shouldEnableControllers;
        transform.GetComponent<ShipMovementController>().enabled = shouldEnableControllers;
        transform.GetComponent<RadarScanner>().enabled = shouldEnableControllers; // Enable radar scanner based on requirements
        if (!shouldEnableControllers)
        {
            GetComponent<TeamController>().KilledShip();
            MasterGunController masterGunController = transform.GetComponent<MasterGunController>();
            masterGunController.CeaseFire();
        }
        // Check if the hull health is zero and perform necessary actions
        if (hullHealth.Health <= 0)
        {
            StartCoroutine(DecreaseFloatingPower());
            GetComponent<TeamController>().KilledShip();
        }
        for (int i = 0; i < guns.Length; i++)
        {
            componentHealth gunHealth = guns[i].GetComponent<componentHealth>();
            if (gunHealth.Health <= 0f && gunHealth != null)
            {
                new WaitForSeconds(Random.Range(1, 5));
                gunHealth.enabled = false;
                StartCoroutine(DelayedExplode(guns[i].transform));
            }
        }
        // Check if the magazine health is zero and perform necessary actions
        if (magazineHealth.Health <= 0 && !isExploded)
        {
            magDet();
            Instantiate(magDetVFX, magazine.transform.position, Quaternion.identity);
            rb.AddExplosionForce(rb.mass * 10, magazine.transform.position, 50, 30f);
            isExploded = true;
        }
        int totalBoilers = boilers.Length;
        int nonFunctionalBoilers = 0;

        foreach (GameObject boiler in boilers)
        {
            if (boiler != null)
            {
                componentHealth healthComponent = boiler.GetComponent<componentHealth>();

                // Check if the boiler has a ComponentHealth component
                if (healthComponent != null)
                {
                    // Check if the health of the boiler is below the functional threshold
                    if (healthComponent.Health < healthComponent.functionalThreshold || healthComponent.isActiveDebuff)
                    {
                        nonFunctionalBoilers++;
                    }
                }
            }
        }

        // Calculate the percentage of non-functional boilers
        float percentageNonFunctional = nonFunctionalBoilers / totalBoilers;

        shipMovementController.moveSpeed *= 1f - percentageNonFunctional;
        shipMovementController.rotationSmoothness *= 1f - percentageNonFunctional;

        if (rudder != null && !rudderAlive)
        {
            componentHealth healthComponent = rudder.GetComponent<componentHealth>();
            if (healthComponent.Health <= 0)
            {
                rudderAlive = true;
                shipMovementController.moveSpeed *= 0.5f;
                shipMovementController.rotationSmoothness *= 0.5f;
            }
        }
    }
    private void Update()
    {
        checkForDamage();
        if (unitController.isSelected)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                foreach (var cube in healthCubes)
                {
                    cube.SetActive(true);
                    healthCubesAreShowing = true;
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.Tab) || !unitController.isSelected)
        {
            foreach (var cube in healthCubes)
            {
                cube.SetActive(false);
                healthCubesAreShowing = false;
            }
        }
        if (healthCubesAreShowing)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, LayerMask.GetMask("Ship"));

            foreach (RaycastHit hit in hits)
            {
                componentHealth target = hit.collider.GetComponent<componentHealth>();
                if (target != null && target.healthCube != null)
                {
                    healthDisplayText(hit.collider.gameObject);
                    floatingTextPrefab.SetActive(true);
                }
                else
                {
                    floatingTextPrefab.SetActive(false);
                }
            }
        }
    }
    private void SpawnHealthCubes(GameObject[] objects)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                BoxCollider boxCollider = obj.GetComponent<BoxCollider>();
                componentHealth objHealth = obj.GetComponent<componentHealth>();
                if (boxCollider != null)
                {
                    GameObject healthIndicator = Instantiate(healthCube, obj.transform.position, obj.transform.rotation);
                    healthCubes.Add(healthIndicator);
                    healthIndicator.transform.parent = transform;
                    healthIndicator.transform.localScale = Vector3.Scale(obj.transform.localScale, boxCollider.size);

                    objHealth.healthCube = healthIndicator;
                    objHealth.healthCubeMaterial = healthIndicator.GetComponent<MeshRenderer>();
                    healthIndicator.SetActive(false);
                }
                if (boxCollider == null)
                {
                    MeshCollider meshCollider = obj.GetComponent<MeshCollider>();
                }
            }
        }
    }
    IEnumerator DecreaseFloatingPower()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        while (buoyancy != null && buoyancy.floatingPower > (buoyancy.floatingPower * 0.01f))
        {
            buoyancy.floatingPower -= decreaseRate;
            floatingPower = buoyancy.floatingPower;

            // Do something with the floating power value, like updating UI or other functionality
            // For example, you might want to update a UI element showing the current floating power.
            rigidbody.AddTorque(randomTorque * rb.mass * decreaseRate * Time.deltaTime, ForceMode.Impulse);
            yield return new WaitForSeconds(0.25f); // Wait for half a second
        }
        yield return new WaitForSeconds(20);
        Destroy(gameObject);
    }
    IEnumerator DelayedExplode(Transform gun)
    {
        yield return new WaitForSeconds(Random.Range(1, 5)); // Wait for a random duration between 1 and 5 seconds

        // Detach and explode the gun after the delay
        GunDetachAndExplode(gun);
    }
    HashSet<Transform> explodedGuns = new HashSet<Transform>();
    void GunDetachAndExplode(Transform gun)
    {
        if (!explodedGuns.Contains(gun)) // Check if the gun has not already exploded
        {
            // Detach all child objects recursively
            List<Transform> children = new List<Transform>();
            GetChildrenRecursive(gun, children);
            foreach (Transform child in children)
            {
                child.parent = null;

                // Add rigidbody to each child object
                Rigidbody rb = child.gameObject.AddComponent<Rigidbody>();
                MeshCollider mc = child.gameObject.AddComponent<MeshCollider>();
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rb.mass = 100f;
                mc.convex = true;
            }

            // Apply explosion force and damage
            Collider[] colliders = Physics.OverlapSphere(gun.position, explosionRadius);
            foreach (Collider col in colliders)
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(100000, gun.position, explosionRadius);
                }

                componentHealth health = col.GetComponent<componentHealth>();
                if (health != null)
                {
                    float distance = Vector3.Distance(gun.position, col.transform.position);
                    float normalizedDistance = 1f - Mathf.Clamp01(distance / 5);
                    float damageAmount = 20 * normalizedDistance;
                    health.ApplyDamage(damageAmount);
                }
            }

            // Instantiate explosion particle effect
            Instantiate(explosionParticlePrefab, gun.position, Quaternion.identity);

            // Add the gun to the exploded guns set
            explodedGuns.Add(gun);

            // Destroy the gun
            foreach (Transform child in children)
            {
                Destroy(child.gameObject, 10);
            }
        }
    }
    void GetChildrenRecursive(Transform parent, List<Transform> children)
    {
        foreach (Transform child in parent)
        {
            children.Add(child);
            GetChildrenRecursive(child, children);
        }
    }
    bool hasExploded = false; // Track whether explosion has occurred
    void magDet()
    {
        if (hasExploded) return; // Prevent multiple explosions

        hasExploded = true;
        Collider[] colliders = Physics.OverlapSphere(magazine.transform.position, explosionRadius);

        foreach (Collider collider in colliders)
        {
            componentHealth healthComponent = collider.GetComponent<componentHealth>();

            if (healthComponent != null)
            {
                healthComponent.ApplyDamage(10000);
            }
        }
        Debug.Log("mag" + colliders.Length);
        GetComponent<TeamController>().KilledShip();
    }

    void healthDisplayText(GameObject component)
    {
        componentHealth componentHealth = component.GetComponent<componentHealth>();
        if (componentHealth != null && textComponent != null)
        {
            // Calculate health percentage
            float healthPercentage = componentHealth.Health / componentHealth.startingHealth;

            // Update the text content
            textComponent.text = component.transform.name + " Health: " + Mathf.RoundToInt(healthPercentage * 100f) + "%";

            // Set the text position to follow the mouse cursor
            RectTransform textRectTransform = floatingTextPrefab.GetComponent<RectTransform>();
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = textRectTransform.position.z; // Ensure the same z position as the camera
            textRectTransform.position = mousePosition;
        }
    }
}