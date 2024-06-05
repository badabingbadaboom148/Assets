using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatHealth : MonoBehaviour
{
    public float Health;
    public float startingHealth;
    public float healthRegen;
    public float healthInterval;

    //public float missileDamageAmount;
    //public float lightCannonDamageAmount;
    //public float mediumCannonDamageAmount;
    //public float heavyCannonDamageAmount;

    public float armorThickness;
    public float internalDensity;

    public ParticleSystem explosionEffect;
    public GameObject destroyedPrefab;

    public ParticleSystem sparks1;
    public ParticleSystem sparks2;
    public ParticleSystem sparks3;

    public ParticleSystem fire1;
    public ParticleSystem fire2;
    public ParticleSystem fire3;
    public ParticleSystem fire4;
    public ParticleSystem fire5;

    public ParticleSystem bigFire1;

    public ParticleSystem smoke1;
    public ParticleSystem smoke2;

    public Buoyancy Buoyancy;

    private void Start()
    {
        startingHealth = Health;
        InvokeRepeating("Heal", 0f, healthInterval);
    }
    public void ApplyDamage(float damage)
    {
        if(Health > 0)
        {
            Health -= damage;

            if (Health <= 0.9 * startingHealth)
            {
            sparks1.Play();
            sparks2.Play();
            sparks3.Play();
            }
            if (Health <= 0.6 * startingHealth)
            {
            smoke1.Play();
            smoke2.Play();
            }
            if (Health <= 0.4 * startingHealth)
            {
            bigFire1.Play();
            }
            if(Health <= 0.2 * startingHealth)
            {
            fire1.Play();
            fire2.Play();
            fire3.Play();
            fire4.Play();
            fire5.Play();
            }
            if (Health <= 0)
            {
                Explosion();
            }
        }
    }
    private void Heal()
    {
        float healLimit = Health * 1.1f;
        if(Health < startingHealth)
        {
            if (Health < healLimit)
            {
                Health += healthRegen;

                // Clamp health to be within the valid range
                Health = Mathf.Clamp(Health, 0f, healLimit);
            }
            Mathf.Clamp(Health, 0f, startingHealth);
        }

        // Stop particle effects based on health thresholds
        if (Health >= 0.9 * startingHealth)
        {
            sparks1.Stop();
            sparks2.Stop();
            sparks3.Stop();
        }
        if (Health >= 0.6 * startingHealth)
        {
            smoke1.Stop();
            smoke2.Stop();
        }
        if (Health >= 0.4 * startingHealth)
        {
            bigFire1.Stop();
        }
        if (Health >= 0.2 * startingHealth)
        {
            fire1.Stop();
            fire2.Stop();
            fire3.Stop();
            fire4.Stop();
            fire5.Stop();
        }
    }
    void Explosion()
    {
        Instantiate(explosionEffect, transform.position, transform.rotation);
        explosionEffect.Play();
        Destroy(gameObject);
        Instantiate(destroyedPrefab, transform.position, transform.rotation);
        //Destroy(destroyedPrefab, 5.0f);
    }
    
}
