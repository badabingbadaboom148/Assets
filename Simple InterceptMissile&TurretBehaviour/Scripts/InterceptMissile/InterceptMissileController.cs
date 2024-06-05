using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterceptMissileController : MonoBehaviour {
	
	[Header("Turret Settings")]
	[Tooltip("Pivot for horizontal rotation")]
	public Transform HorizontalPivot;

	[Header("Rotation Settings")]
	[Tooltip("If you want to limit turret rotation")]
	public bool RotationLimit;

	[Tooltip("Right rotation limit")]
	[Range(0,180)]
	public float RightRotationLimit; 

	[Tooltip("Left rotation limit")]
	[Range(0,180)]
	public float LeftRotationLimit; 

	[Tooltip("Turning speed")]
	[Range(0,300)]
	public float TurnSpeed;

	[Header("Missile settings")]
	[Tooltip("How many missile in turret")]
	public float MissileCount;

	[Tooltip("Launcher Spot")]
	public Transform[] LaunchSpot;

	[Tooltip("Missile Prefab")]
	public InterceptMissile missile;

	[HideInInspector]
	public Transform target; // Target position

	[HideInInspector]
	public float loadedMissileCount; // Count of loaded missile on launcher

	private List<InterceptMissile> loadedMissile = new List<InterceptMissile>(); // loaded missile list on launcher
	
	
	private void Start()
	{
		target = null;
		SpawnMissile(); // spawn missile
	}

	IEnumerator RespawnMissile()
	{	
		yield return new WaitForSeconds(2);
		if(MissileCount <= 0) yield return 0;
		SpawnMissile();
	}

	private void SpawnMissile()
	{	
		if(LaunchSpot.Length == 0) // check for missile launchSpot
		{
			Debug.Log("No LaunchSpot found, Please drag it into this script");
		}
		
		foreach(Transform spot in LaunchSpot)
		{	
			if(MissileCount <= 0) return;
			InterceptMissile newMissile = Instantiate(missile, spot.position, spot.rotation);
			newMissile.transform.parent = spot;

			Vector3 offset = new Vector3(0,0,0.2f); 
			newMissile.transform.localPosition = offset; // Note: optional position

			loadedMissile.Add(newMissile);
			loadedMissileCount ++;
			MissileCount --;
		}
	}
	public void SetTargetMissile(Transform targetPosition)
	{	
		this.target = targetPosition;
		Launch(targetPosition);
	}

	private void Launch(Transform targetPosition)
	{			
		loadedMissile[(int)loadedMissileCount - 1].Launch(targetPosition); // Launch missile according to its sequence in list
		loadedMissile[((int)loadedMissileCount - 1)].transform.parent = null; 
		loadedMissile.Remove(loadedMissile[((int)loadedMissileCount - 1)]); // Remove missile from loaded missile list
		loadedMissileCount --;

		if(loadedMissileCount <= 0)   
			StartCoroutine(RespawnMissile()); //if loaded missile on launcher is null Respawn
	}

	
}
