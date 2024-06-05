using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flakMunition : MonoBehaviour
{
    public ParticleSystem detonationPS;
    public float damageRadius;
    public float damageAmount;
    public int damageRays; // Changed from float to int for the number of spheres

    // Set a time delay for detonation
    public float detonationDelay = 2.0f;

    void Detonate()
    {
        // Instantiate the detonation particle system
        Instantiate(detonationPS, transform.position, Quaternion.identity);

        // Spawn damage spheres
        SpawnDamageSpheres();

        // Destroy the flakMunition GameObject
        Destroy(gameObject);
    }

    void Start()
    {
        float detonationDelayModified = detonationDelay + Random.Range(-0.1f, 0.25f);
        Invoke("Detonate", detonationDelayModified);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Destroy the flakMunition GameObject when it collides with something
        Destroy(gameObject);
    }

    void Update()
    {
        // Check if the Y-coordinate is below 0
        if (transform.position.y < 0)
        {
            // Invoke Detonate to handle destruction logic
            Destroy(gameObject);
        }
    }

    void SpawnDamageSpheres()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, damageRadius, transform.forward, out hit, damageRadius))
        {
            // Apply damage to the hit object if it has a health component
            Missile health = hit.collider.GetComponent<Missile>();
            if (health != null)
            {
                health.DealDamage(damageAmount);
            }
        }
    }
}
