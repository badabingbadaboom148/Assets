﻿using UnityEngine;

public class LaunchMissile : MonoBehaviour {

	[Tooltip("Target Transform")]
	public Transform Target;

	[Tooltip("Missile to instantiate")]
	public Missile Missile;

	[Tooltip("Position for missile to launch")]
	public Transform[] LaunchSpot;

	[Tooltip("Missile Fire Rate / how frequently to launch the missile")]
	public float MissileFireRate;

	public float MissileCount;

	private bool Fire; // Get value from user whether to fire or not
	private float nextlaunch; // Store next "MissileFireRate" 

	
	private void Update()
	{	

		Fire =  Input.GetKey(KeyCode.Space);

		// Only fire the missile when user give some input and time more than next launch time;
		if(Fire && Time.time >= nextlaunch && MissileCount > 0)
		{	
			nextlaunch = Time.time + MissileFireRate;
			FireMissile();
			MissileCount = MissileCount - 1;
		}
	}

	private void FireMissile()
	{	
		// instantiate missile and give it a taregt;
		for(int i = 0; i < LaunchSpot.Length; i++)
		{
            if (MissileCount > 0)
            {
				Missile newMissile = Instantiate(Missile, LaunchSpot[i].position, LaunchSpot[i].rotation);
				//newMissile.Target = Target;
			}
		}
		
	}
}
