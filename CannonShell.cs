using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CannonShell : MonoBehaviour
{
    public float damage;
    public float armorPen;
    public float damageFragments;
    public float explosionRadius = 5f;
    public float underwaterTravelDistance;

    public float projectileSpeed;
    public float scaleDecreaseRate = 0.1f;
    public float maxScale = 1.0f;
    private float currentScale;
    public bool isInsideWater = false;

    public ParticleSystem cannonSmoke;
    public ParticleSystem bounceEffect;
    public ParticleSystem penEffect;
    public ParticleSystem noPen;

    public ParticleSystem waterSplash;

    public float armorModifier;
    public float armorStrength = 1f;

    private float targetArmorThickness;
    private bool isUnderWater;
    private BoxCollider boxCollider; // Reference to the BoxCollider component
    Rigidbody rb;

    public GameObject hitText;
    private Camera mainCamera;
    private GameObject mainCanvas;

    private void Start()
    {
        currentScale = maxScale;
        rb = gameObject.GetComponent<Rigidbody>();

        // Get the BoxCollider component and disable it initially
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.enabled = false;

        // Enable the BoxCollider after a delay of 0.25 seconds
        Invoke("EnableCollider", 0.05f);

        hitText.SetActive(false);
        mainCamera = Camera.main;

        mainCanvas = GameObject.Find("MainCanvas");
    }

    private void EnableCollider()
    {
        // Enable the BoxCollider
        boxCollider.enabled = true;
    }
    private void Update()
{
    if (gameObject.transform.position.y <= 1 && !isUnderWater)
    {
        Instantiate(waterSplash, transform.position, Quaternion.identity);
        isUnderWater = true;
    }
    if (gameObject.transform.position.y <= underwaterTravelDistance)
    {
        cannonSmoke.transform.parent = null;
        Destroy(cannonSmoke, 5f);
        Destroy(gameObject);
    }

    // Adjust rotation to align with velocity
    if (GetComponent<Rigidbody>().velocity != Vector3.zero)
    {
        transform.rotation = Quaternion.LookRotation(GetComponent<Rigidbody>().velocity);
    }
}


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.root.GetComponent<TeamController>())
        {
            // Apply damage to the target.
            ShipHealth warshiptarget = collision.transform.root.GetComponent<ShipHealth>();
            airshipHealth airship = collision.transform.root.GetComponent<airshipHealth>();

            if (warshiptarget != null)
            {
                targetArmorThickness = warshiptarget.armorThickness;
                
                Vector3 projectileDirection = transform.forward;

                Vector3 normalVector = collision.contacts[0].normal;
                float angle = 999f;
                RaycastHit hit;
                if (Physics.Raycast(transform.position, projectileDirection, out hit, 1))
                {
                    angle = Vector3.Angle(transform.position, hit.normal);
                    Debug.Log(angle);
                }
                float radians = angle * Mathf.Deg2Rad;

                float effectiveThickness = targetArmorThickness / Mathf.Cos(radians);
                float modifiedThickness = Mathf.Min(effectiveThickness, 2.5f * targetArmorThickness);
                modifiedThickness = Mathf.Clamp(modifiedThickness, targetArmorThickness, modifiedThickness);
                if (angle <= 75 && armorPen < modifiedThickness)
                {
                    ParticleSystem bounceEffectPS = Instantiate(bounceEffect, transform.position, transform.rotation);
                    damage = 0;
                }
                if (angle >= 75 && armorPen <= modifiedThickness)
                {
                    ParticleSystem bounceEffectObject = Instantiate(penEffect, transform.position, transform.rotation);
                    SpawnPartialDamageCone(modifiedThickness);
                }
                if (armorPen >= modifiedThickness)
                {
                    ParticleSystem yesPen = Instantiate(noPen, transform.position, transform.rotation);
                    yesPen.transform.localScale = new Vector3(10, 10, 10);
                    SpawnFullDamageCone(modifiedThickness, warshiptarget.internalDensity);
                }
                else if (armorPen < modifiedThickness && angle >= 75)
                {
                    // Ricochet effect
                    ParticleSystem bounceEffectObject = Instantiate(penEffect, transform.position, transform.rotation);
                    SpawnPartialDamageCone(modifiedThickness);
                }
                else
                {
                    // No penetration or ricochet effect
                    damage = 0;
                }
                Vector3 hitPosition = collision.contacts[0].point;

                // Show hit text at the hit position
                ShowHitText(hitPosition);

                cannonSmoke.transform.parent = null;
                Destroy(cannonSmoke, 5f);
                Destroy(gameObject);
            }
            if (airship != null)
            {
                targetArmorThickness = airship.armorThickness;
                Vector3 projectileDirection = transform.forward;

                Vector3 normalVector = collision.contacts[0].normal;
                float angle = Vector3.Angle(normalVector, projectileDirection);
                float radians = angle * Mathf.Deg2Rad;

                float effectiveThickness = targetArmorThickness / Mathf.Cos(radians);
                float modifiedThickness = Mathf.Min(effectiveThickness, 2.5f * targetArmorThickness);
                modifiedThickness = Mathf.Clamp(modifiedThickness, targetArmorThickness, modifiedThickness);
                if (angle >= 75 && armorPen < modifiedThickness)
                {
                    ParticleSystem bounceEffectPS = Instantiate(bounceEffect, transform.position, transform.rotation);
                    damage = 0;
                }
                if (angle <= 75 && armorPen <= modifiedThickness)
                {
                    ParticleSystem bounceEffectObject = Instantiate(penEffect, transform.position, transform.rotation);
                    bounceEffectObject.transform.localScale = new Vector3(3, 3, 3);
                    SpawnPartialDamageCone(modifiedThickness);
                }
                if (armorPen >= modifiedThickness)
                {
                    ParticleSystem yesPen = Instantiate(noPen, transform.position, transform.rotation);
                    SpawnFullDamageCone(modifiedThickness, airship.internalDensity);
                }
                else if (armorPen < modifiedThickness && angle <= 75)
                {
                    // Ricochet effect
                    ParticleSystem bounceEffectObject = Instantiate(penEffect, transform.position, transform.rotation);
                    bounceEffectObject.transform.localScale = new Vector3(3, 3, 3);
                    SpawnPartialDamageCone(modifiedThickness);
                }
                Vector3 hitPosition = collision.contacts[0].point;

                // Show hit text at the hit position
                ShowHitText(hitPosition);
                cannonSmoke.transform.parent = null;
                Destroy(cannonSmoke, 5f);
                Destroy(gameObject);
            }
        }
        else
        {
            Instantiate(penEffect, transform.position, transform.rotation);
            cannonSmoke.transform.parent = null;
            Destroy(cannonSmoke, 5f);
            Destroy(gameObject);
        }
    }
    private void SpawnFullDamageCone(float modifiedThickness, float internalDensity)
    {
        Collider[] hits = Physics.OverlapCapsule(transform.position, transform.position + transform.forward * ((armorPen - modifiedThickness) / internalDensity), 0.25f);
        foreach (var hit in hits)
        {
            componentHealth health = hit.transform.GetComponent<componentHealth>();
            if (health != null)
            {
                health.ApplyDamage(damage);
                Debug.Log("Applied damage: " + damage);
                Debug.Log("damage " + hits.Length);
                return;
            }
        }
        Debug.DrawLine(transform.position, transform.position + transform.forward * (((armorPen - modifiedThickness) / internalDensity)), Color.red, 5f);
    }
    private void SpawnPartialDamageCone(float modifiedThickness)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider collider in colliders)
        {
            componentHealth target = collider.GetComponent<componentHealth>();
            Debug.Log("override");
            if (target != null)
            {
                if (target.isExternal)
                {
                    float rayDamage = damage * 0.25f;
                    target.ApplyDamage(rayDamage);
                    Debug.Log("Applied damage: " + rayDamage);
                    return;
                }
                if (!target.isExternal)
                {
                    float rayDamage = (damage * 0.1f);
                    target.ApplyDamage(rayDamage);
                    Debug.Log("Applied damage: " + rayDamage);
                    return;
                }
            }
        }
        for (int i = 0; i < damageFragments; i++)
        {
            // Calculate random direction within a cone
            float coneAngle = 60.0f; // Adjust the cone angle as needed
            Vector3 randomDirection = Quaternion.Euler(Random.Range(-coneAngle, coneAngle), Random.Range(0, 90), 0) * transform.forward;

            RaycastHit[] shrapnelhits = Physics.RaycastAll(transform.position, randomDirection, armorPen - modifiedThickness);

            foreach (RaycastHit hits in shrapnelhits)
            {
                // Draw debug line to visualize the ray for a specific distance
                Debug.DrawRay(transform.position, randomDirection * hits.distance, Color.red, 1f);
                componentHealth target = hits.transform.GetComponent<componentHealth>();
                if (target != null)
                {
                    if (target.isExternal)
                    {
                        float rayDamage = damage / shrapnelhits.Length;
                        target.ApplyDamage(rayDamage);
                        Debug.Log("Applied damage: " + rayDamage);
                    }
                    if (!target.isExternal)
                    {
                        float rayDamage = (damage * 0.25f) / shrapnelhits.Length;
                        target.ApplyDamage(rayDamage);
                        Debug.Log("Applied damage: " + rayDamage);
                    }
                }
                break;
            }
        }
    }
    public void ShowHitText(Vector3 hitPosition)
    {
        if (mainCanvas != null)
        {
            // Convert world position to screen space
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(hitPosition);
            screenPosition = new Vector3(screenPosition.x, screenPosition.y, 0);
            GameObject newHitText = Instantiate(hitText, transform.position, transform.rotation);
            newHitText.transform.rotation = Quaternion.Euler(0, 0, 0);

            newHitText.transform.position = screenPosition;
            newHitText.transform.parent = mainCanvas.transform;
            newHitText.gameObject.SetActive(true);
        }
    }
}