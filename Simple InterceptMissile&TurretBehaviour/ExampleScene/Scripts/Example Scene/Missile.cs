﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Missile : MonoBehaviour {
	[Header("General Parameter")]

	[Tooltip(" Missile traveling speed")]
	public float MissileSpeed = 0f;

	[Tooltip("Missile acceleration during missile motor is active")]
	public float Acceleration = 20f;

	[Tooltip("Time for missile automatically explode")]
	public float MissileLifeTime = 20f;

	[Tooltip("Time delay before activate the missile")]
	public float LaunchDelayTime = 3f;

	[Tooltip("Time delay before start to guide(tracking target) missile")]
	public float TrackingDelay = 3f;

	[Tooltip("Initial force before activate the missile")]
	public float InitialLaunchForce = 15f;

	[Tooltip("Motor life time before it stops accelerating")]
	public float MotorLifeTime = 15f;

	[Tooltip("Missile turn rate towards target")]
	public float TurnRate = 90;

	[Tooltip("Missile Explsotion GameObject")]
	public GameObject MissileExplosion;
	public ParticleSystem pdHitExplode;

	[Tooltip("Missile Flame trail")]
	public ParticleSystem MissileFlameTrail;

	[Tooltip("Missile launch Sound effect")]
	public AudioClip LaunchSFX;
	public AudioClip missileLoop;
	private AudioSource audioSource;

	public Transform Target; // Missile's target transform;

	public float health;
	public float damage;
	public float damageFragments = 1500;
	public ParticleSystem shrapnelOverpen;

	[Header("DEVIATION")]
	[SerializeField] private float _deviationAmount = 50;
	[SerializeField] private float _deviationSpeed = 2;
	public bool canDeviate = true;


	private  bool targetTracking = false; // Bool to check whether the missile can track the target;
	private bool missileActive = false; // Bool  to check if missile is active or not;
	private bool motorActive = false; // Bool  to check if motor is active or not;
	private bool explosionActive = false; // Bool to activate the explosive;
	private float MissileLaunchTime; // Get missile launch time;
	private float MotorActiveTime; // Get missile Motor active time;
	private Quaternion guideRotation; // Store rotation to guide the missile;
	private Rigidbody rb;

	[Header("Seeker Parameters")]
	[Tooltip("Angle of the seeker cone")]
	public float SeekerAngle = 90f;
	private int missileLayer;

	private TeamController teamController;

	public static event Action<Missile> OnMissileDestroyed;
	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		MissileLaunchTime = Time.time;
		ActivateMissile();
		targetTracking = true;
		audioSource = GetComponent<AudioSource>();

		// Retrieve the layer number assigned to missiles
		missileLayer = gameObject.layer;

		teamController = transform.root.GetComponent<TeamController>();
		//RadarScanner.ships.Add(transform.GetComponent<TeamController>());
	}

	public void SetTarget(GameObject target)
    {
		Target = target.transform;
    }
	private void ActivateMissile()
	{
		if (canDeviate)
        {
			missileActive = true;

			motorActive = true;
			MotorActiveTime = Time.time;
			MissileFlameTrail.Play();
			Physics.IgnoreLayerCollision(missileLayer, missileLayer, true);
		}
	}
	private void FixedUpdate()
	{
		Run();
		// Guide missile
		GuideMissile();
		if (audioSource != null && missileLoop != null && !audioSource.isPlaying)
		{
			audioSource.clip = missileLoop;
			audioSource.loop = true;
			audioSource.Play();
		}
		if (transform.position.y < -5)
        {
			DestroyMissile();
			Instantiate(MissileExplosion, transform.position, transform.rotation);
		}
		if (health <= 0)
		{
			DestroyMissile();
			Destroy(MissileFlameTrail, 5f);
			MissileFlameTrail.transform.parent = null;
			MissileFlameTrail.Stop();
			Instantiate(pdHitExplode, transform.position, transform.rotation);
		}
	}
	public void DealDamage(float damageAmount)
    {
		health -= damageAmount;
	}
	private bool hasCausedDamage = false;

	private void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.tag == "20mm")
		{
			// Deal damage to the missile
			DealDamage(2);
		}
		if (col.transform.GetComponent<RadarScanner>() != null)
		{
			hasCausedDamage = true;
			DestroyMissile();
			SpawnDamageSphere(col.gameObject);
			Instantiate(MissileExplosion, transform.position, transform.rotation);
		}
		else if (col.gameObject.tag == "Obstacle")
		{
			MissileFlameTrail.transform.parent = null;
			DestroyMissile();
			MissileFlameTrail.Stop();
			Instantiate(MissileExplosion, transform.position, transform.rotation);
			Debug.Log("missile");
		}
	}

	private void Run()
	{	
		// Check if missile motor is still active ?
		if(Since(MotorActiveTime) > MotorLifeTime)
			motorActive = false; // if motor exceed the "MotorActiveTime" duration : motor will be stopped
		else 
			motorActive = true;  // if not : motor continuing running
		
		// if missile active move it
		if(!missileActive)  return;
		
			// Keep missile accelerating when motor is still active
			if(motorActive)
			MissileSpeed += Acceleration * Time.deltaTime;
			
			rb.velocity = transform.forward * MissileSpeed;
			
			// Rotate missile towards target according to "guideRotation" value
			if(targetTracking)
				transform.rotation = Quaternion.RotateTowards(transform.rotation, guideRotation, TurnRate * Time.deltaTime);

			if(Since(MissileLaunchTime) > MissileLifeTime) // Destroy Missile if it more than live time
				DestroyMissile();
	}

	// Guide missile towards target
	private void GuideMissile()
	{
		AddDeviation();
		SeekShip();
	}
	private void SeekShip()
	{
		float coneLength = 1500;
		float detectionRadius = SeekerAngle / 2.0f; // Define the radius based on the seeker angle
		int layerMask = LayerMask.GetMask("Ship"); // Define a layer mask for the ships

		Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, layerMask);

		foreach (Collider hit in hits)
		{
			Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
			float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

			if (hit.GetComponent<RadarScanner>() == null) continue;
			if (hit.GetComponent<RadarScanner>() != null)
			{
				RaycastHit raycastHit;
				if (Physics.Raycast(transform.position, directionToTarget, out raycastHit, coneLength))
				{
					componentHealth target = raycastHit.collider.GetComponent<componentHealth>();
					TeamController isFriendly = raycastHit.collider.transform.root.GetComponent<TeamController>();

					if (raycastHit.collider.CompareTag("Ship") && isFriendly.isFriendly != teamController.isFriendly && target != null)
					{
						Target = raycastHit.collider.transform;
						guideRotation = Quaternion.LookRotation(directionToTarget, transform.up);
						canDeviate = false; // Set canDeviate to false when a target is spotted
						return; // Exit the method immediately after finding a valid target
					}
				}
			}
		}

		// If no target is found, add deviation
		AddDeviation();
	}
	private Vector3 previousTargetPosition;

	private void CalculateRelativePosition()
	{
		if (Target != null)
		{
			// Calculate the current relative position of the target
			Vector3 currentRelativePosition = Target.transform.position - transform.position;

			// Calculate the velocity vector based on the change in position between the current and previous frames
			Vector3 targetVelocity = (Target.transform.position - previousTargetPosition) / Time.deltaTime;

			// Estimate the time it takes for the missile to reach the target
			float estimatedTimeToTarget = currentRelativePosition.magnitude / MissileSpeed;

			// Predict the target's future position after the estimated time
			Vector3 predictedTargetPosition = Target.transform.position + targetVelocity * estimatedTimeToTarget;

			// Store the current position of the target for the next frame
			previousTargetPosition = Target.transform.position;

			// Calculate a deviation vector
			Vector3 deviation = new Vector3(UnityEngine.Random.Range(-_deviationAmount * 0.5f, _deviationAmount * 0.5f),
											0,
											0);

			// Transform the deviation vector into world space and scale it by the deviation amount
			Vector3 predictionOffset = transform.TransformDirection(deviation) * _deviationAmount;

			// Adjust the relative position based on the predicted target position and deviation
			currentRelativePosition = predictedTargetPosition - transform.position + predictionOffset;

			// Set the rotation based on the adjusted relative position
			guideRotation = Quaternion.LookRotation(currentRelativePosition, transform.up);
		}
	}


	private void AddDeviation()
	{
		if (!canDeviate)
        {
			_deviationAmount = 0;
			_deviationSpeed = 0;
		}
		CalculateRelativePosition();
	}
	// Get the "Since" time from the input/parameter value
	private float Since(float Since)
	{
		return Time.time - Since;
	}

	// Destroy Missile
	private void DestroyMissile()
	{
		Destroy(MissileFlameTrail, 5f);
		MissileFlameTrail.transform.parent = null;
		MissileFlameTrail.Stop();
		RadarScanner.ships.Remove(transform.root.GetComponent<TeamController>());
		Destroy(gameObject);

		if (OnMissileDestroyed != null)
		{
			OnMissileDestroyed(this);
		}
	}
	private void SpawnDamageSphere(GameObject hitObject)
	{
		float damageConeAngle = 70f;
		for (int i = 0; i < damageFragments; i++)
		{
			float halfConeAngle = damageConeAngle / 2.0f;

			// Generate random angles within the cone
			float randomYaw = UnityEngine.Random.Range(-halfConeAngle, halfConeAngle);
			float randomPitch = UnityEngine.Random.Range(-halfConeAngle, halfConeAngle);

			// Create the random rotation
			Quaternion randomRotation = Quaternion.Euler(randomPitch, randomYaw, 0);

			// Apply the random rotation to the forward vector
			Vector3 randomDirection = transform.rotation * randomRotation * transform.forward * -1;

			// Calculate the raycast distance
			float raycastDistance = 50;
			Debug.Log(transform.forward);
			// Perform the raycast
			RaycastHit[] hits = Physics.RaycastAll(transform.position, randomDirection, raycastDistance, LayerMask.GetMask("Ship"));

			foreach (RaycastHit hit in hits)
			{
				// Check if the hit object has the "Ship" tag
				if (hit.collider.CompareTag("Ship"))
				{
					componentHealth target = hit.collider.GetComponent<componentHealth>();

					if (target != null && target.Health >= target.functionalThreshold)
					{
						float rayDamage = damage / damageFragments;
						target.ApplyDamage(rayDamage);
						Debug.Log("Applied damage: " + rayDamage);
						Debug.DrawRay(transform.position, randomDirection * hit.distance, Color.green, 3f);
						break;
					}
				}
			}
			Debug.DrawRay(transform.position, randomDirection * 15, Color.red, 3f);
		}
	}
}