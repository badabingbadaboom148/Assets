using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class componentHealth : MonoBehaviour
{
    public float Health;
    public float startingHealth;
    public float maximumHealth;
    public float healAmount;
    public ParticleSystem disabledSmoke;

    public bool isActiveDebuff = false;

    // Chance of Fire debuff occurring (in percentage)
    public float fireDebuffChance = 5f;
    public float functionalThreshold;

    // Reference to the ParticleSystem
    public ParticleSystem fireParticleSystem;
    public bool isDisableable;
    public bool isExternal;

    ParticleSystem currentFirePS;

    public GameObject healthCube;
    public MeshRenderer healthCubeMaterial;
    public GameObject floatingTextPrefab;

    private bool disabledSmokeSpawned = false;
    private void Start()
    {
        startingHealth = Health;
        functionalThreshold = startingHealth * 0.1f;
        if (healthCube != null)
        {
            healthCubeMaterial = healthCube.GetComponent<MeshRenderer>();
        }
    }

    public void ApplyDamage(float damage)
    {
       if (transform.root.GetComponent<airshipHealth>() == null)
        {
            transform.root.GetComponent<ShipHealth>().checkForDamage();
        }
        else
        {
            transform.root.GetComponent<airshipHealth>().checkForDamage();
        }
        if (Health <= 0)
        {
            Health = 0;
            isActiveDebuff = true;
            transform.GetComponent<componentHealth>().enabled = false;
        }
        if (Health > 0)
        {
            Health -= damage;
            // Check for Fire debuff
            if (Random.Range(0f, 100f) < fireDebuffChance && fireParticleSystem != null)
            {
                ApplyFireDebuff();
            }

            if (Health <= 0)
            {
                gameObject.GetComponent<Collider>().enabled = false;
            }
            else
            {
                gameObject.GetComponent<Collider>().enabled = true;
            }
        }
        if(Health < functionalThreshold)
        {
            Health -= damage / 2;
            if (isDisableable)
            {
                if (disabledSmoke != null && !disabledSmokeSpawned)
                {
                    ParticleSystem disabledSmokePS = Instantiate(disabledSmoke, transform.position, transform.rotation);
                    disabledSmokeSpawned = true;
                    disabledSmokePS.transform.parent = transform.root;
                    disabledSmokePS.Play();
                }
                GT2.navalGunController gunController = transform.GetComponent<GT2.navalGunController>();
                FlakTurret flakTurret = transform.GetComponent<FlakTurret>();
                AntiMissileGunController AMG = transform.GetComponent<AntiMissileGunController>();
                AudioSource audio = transform.GetComponent<AudioSource>();
                if (gunController != null)
                {
                    // If the component exists, disable it
                    gunController.enabled = false;
                }
                if (flakTurret != null)
                {
                    // If the component exists, disable it
                    flakTurret.enabled = false;
                }
                if (AMG != null)
                {
                    // If the component exists, disable it
                    AMG.enabled = false;
                }
                if (audio != null)
                {
                    audio.enabled = false;
                }
            }
            isActiveDebuff = false;
        }
        if (healthCubeMaterial != null)
        {
            float healthPercentage = Health / startingHealth;
            Color lerpedColor = Color.Lerp(Color.red, Color.green, healthPercentage);

            if (healthPercentage <= 0)
            {
                healthCubeMaterial.material.color = Color.magenta;
            }
            else
            {
                healthCubeMaterial.material.color = lerpedColor;
            }
        }
    }

    IEnumerator damageFlash()
    {
        Color startingColor = healthCubeMaterial.material.color;
        healthCubeMaterial.material.color = Color.white;
        yield return new WaitForSeconds(0.5f);
        healthCubeMaterial.material.color = startingColor;
    }

    void ApplyFireDebuff()
    {
        // Fire debuff decreases health by 5 for a random duration between 10-45 seconds
        float debuffDuration = Random.Range(10f, 45f);

        // Instantiate and play the fire particle system
        
        if (fireParticleSystem == null)
        {
            currentFirePS = Instantiate(fireParticleSystem, transform.position, transform.rotation);
            currentFirePS.transform.parent = transform.root;
            currentFirePS.transform.localScale = new Vector3(1, 1, 1);
            currentFirePS.Play();
        }
        fireParticleSystem.Play();
        StartCoroutine(FireDebuffRoutine(debuffDuration));
    }

    IEnumerator FireDebuffRoutine(float duration)
    {
        // Apply debuff logic here (e.g., decrease health by 5 over time)
        float startTime = Time.time;

        while (Time.time - startTime < duration && Health > functionalThreshold)
        {
            Health -= 1f * Time.deltaTime;
            yield return null;
        }

        // Stop the particle system after the debuff duration
        fireParticleSystem.Stop();
        if (currentFirePS != null)
        {
            currentFirePS.Stop();
        }
    }

    void Heal()
    {
        float oldMaximumHealth = maximumHealth;
        if (Health > 0)
        {
            maximumHealth = Health * 1.1f;
            Health += healAmount;

            if (maximumHealth > startingHealth)
            {
                maximumHealth = startingHealth;
            }
            if (maximumHealth > oldMaximumHealth)
            {
                maximumHealth = oldMaximumHealth;
            }
        }
    }
    IEnumerator Restore()
    {
        // Wait for 1 minute
        yield return new WaitForSeconds(60f);

        // Gradually restore to 10% of starting health
        float restoreDuration = 10f; // Adjust the duration as needed
        float startTime = Time.time;

        while (Time.time - startTime < restoreDuration)
        {
            Health = Mathf.Lerp(Health, startingHealth * 0.1f, Time.deltaTime / restoreDuration);
            yield return null;
        }
    }

}