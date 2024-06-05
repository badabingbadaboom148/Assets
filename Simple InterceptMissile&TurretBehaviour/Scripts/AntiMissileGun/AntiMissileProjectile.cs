using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiMissileProjectile : PoolObject
{

	[Header("Projectile settings")]
	[Tooltip("Projectile traveling speed")]
	[HideInInspector]
	public float Speed;

	[Tooltip("Projectile life time")]
	public float TimeTodestroy;

	[Tooltip("Projectile Explosion FX (Optional)")]
	public GameObject Explosion;
	public GameObject waterSplash;

	private void Update()
	{
		// Move projectile
		transform.Translate(Vector3.forward * Speed * Time.deltaTime);
		if (transform.position.y <= 0)
        {
			Instantiate(waterSplash, transform.position, Quaternion.identity);
			Destroy(gameObject);
		}
	}

	// Destroy gameobject when collisin happen
	void OnCollisionEnter(Collision col)
	{
		Instantiate(Explosion, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}
}
