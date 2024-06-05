using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class airshipHealth : MonoBehaviour
{
    public GameObject[] command;
    public GameObject[] boilers;
    public GameObject[] propellors;
    public GameObject[] guns;
    public GameObject magazine;
    public GameObject hull;
    public GameObject[] gasbags;
    public GameObject[] radarDish;

    public float armorThickness;
    public float internalDensity = 0.5f;

    componentHealth magazineHealth;
    componentHealth hullHealth;
    public GameObject magDetVFX;
    public ParticleSystem explosionParticlePrefab;
    bool isExploded = false;
    public float explosionRadius = 200f;

    private Rigidbody rb;

    private UnitController unitController;


    public GameObject healthCube;
    private List<GameObject> healthCubes = new List<GameObject>();
    public bool healthCubesAreShowing;

    public GameObject floatingTextPrefab;
    private Text textComponent;
    private void Start()
    {
        magazineHealth = magazine.GetComponent<componentHealth>();
        hullHealth = hull.GetComponent<componentHealth>();
        rb = transform.GetComponent<Rigidbody>();
        unitController = transform.GetComponent<UnitController>();

        SpawnHealthCubes(command);
        SpawnHealthCubes(boilers);
        SpawnHealthCubes(propellors);
        SpawnHealthCubes(guns);
        SpawnHealthCubes(new GameObject[] { magazine, hull });
        SpawnHealthCubes(gasbags);
        SpawnHealthCubes(radarDish);
        floatingTextPrefab.SetActive(false);
        textComponent = floatingTextPrefab.transform.GetComponent<Text>();
    }
    public void checkForDamage()
    {
        bool allCommandsZeroHealth = true;
        for (int i = 0; i < command.Length; i++)
        {
            componentHealth commandHealth = command[i].GetComponent<componentHealth>();

            // Check if any command has health greater than zero
            if (commandHealth != null && commandHealth.Health > 0)
            {
                allCommandsZeroHealth = false;
                break;  // No need to check further once a command with health > 0 is found
            }
        }
        if (allCommandsZeroHealth)
        {
            transform.GetComponent<UnitController>().enabled = false;
            transform.GetComponent<airship>().enabled = false;
            transform.GetComponent<RadarScanner>().enabled = false;
            transform.GetComponent<TeamController>().KilledShip();
            MasterGunController masterGunController = transform.GetComponent<MasterGunController>();
            masterGunController.CeaseFire();
        }
        if (hullHealth.Health <= 0)
        {

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
        if (magazineHealth.Health <= 0)
        {
            if (!isExploded)
            {
                magDet();
                Instantiate(magDetVFX, magazine.transform.position, Quaternion.identity);
                rb.AddExplosionForce(10000, magazine.transform.position, 10, 3.0f);
                isExploded = true;
            }
        }
        if (transform.position.y <= 20)
        {
            Destroy(gameObject);
        }
        bool allRadarZeroHealth = false;
        for (int i = 0; i < radarDish.Length; i++)
        {
            componentHealth radarHealth = radarDish[i].GetComponent<componentHealth>();
            if (radarHealth != null && radarHealth.Health > 0)
            {
                allRadarZeroHealth = false;
            }
            if (radarHealth != null && radarHealth.Health <= 0)
            {
                radarDish[i].GetComponent<RadarRotation>().enabled = false;
            }
        }
        if (allRadarZeroHealth)
        {
            transform.GetComponent<RadarScanner>().enabled = false;
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
            bool hitFound = false;

            foreach (RaycastHit hit in hits)
            {
                componentHealth target = hit.collider.GetComponent<componentHealth>();
                if (hit.collider.tag == "Ship" && target != null && target.healthCube != null)
                {
                    healthDisplayText(hit.collider.gameObject);
                    hitFound = true;
                    floatingTextPrefab.SetActive(hitFound);
                }
            }
        }
    }
    IEnumerator DelayedExplode(Transform gun)
    {
        yield return new WaitForSeconds(Random.Range(1, 5)); // Wait for a random duration between 1 and 5 seconds

        // Detach and explode the gun after the delay
        GunDetachAndExplode(gun);
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
                    float damageAmount = 25 * normalizedDistance;
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
    void healthDisplayText(GameObject component)
    {
        componentHealth componentHealth = component.GetComponent<componentHealth>();
        if (componentHealth != null && textComponent != null)
        {
            // Calculate health percentage
            float healthPercentage = componentHealth.Health / componentHealth.startingHealth;

            // Update the text content
            textComponent.text = "Health: " + Mathf.RoundToInt(healthPercentage * 100f) + "%";

            // Set the text position to follow the mouse cursor
            RectTransform textRectTransform = floatingTextPrefab.GetComponent<RectTransform>();
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = textRectTransform.position.z; // Ensure the same z position as the camera
            textRectTransform.position = mousePosition;
        }
    }
}