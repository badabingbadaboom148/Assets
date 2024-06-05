// tubeSpawnMissile.cs
using System.Collections;
using UnityEngine;

public class tubeSpawnMissile : MonoBehaviour
{
    public Transform hatchSwivelPoint;
    public GameObject launchParticles;
    public GameObject missilePrefab;
    public float launchForce = 10f;
    private GameObject target;
    public float rotationSpeed = 60f;
    public float missileOrientation;

    void Start()
    {
        // Assuming Missile script is attached to the missilePrefab
        missilePrefab.GetComponent<Missile>();
    }

    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
    }

    public void Fire()
    {
        StartCoroutine(LaunchMissile());
    }

    IEnumerator LaunchMissile()
    {
        // Rotate the hatchSwivelPoint (you can use your existing code for rotation here)
        Quaternion targetRotation = Quaternion.Euler(0f, -90f, 0f);
        while (Quaternion.Angle(hatchSwivelPoint.rotation, targetRotation) > 0.01f)
        {
            hatchSwivelPoint.rotation = Quaternion.RotateTowards(hatchSwivelPoint.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        // Instantiate the missile
        GameObject newMissile = Instantiate(missilePrefab, transform.position, transform.rotation * Quaternion.Euler(90f * missileOrientation, 0f, 90f));
        Rigidbody missileRigidbody = newMissile.GetComponent<Rigidbody>();
        Missile missileScript = newMissile.GetComponent<Missile>();
        newMissile.GetComponent<Missile>().enabled = false;
        newMissile.GetComponent<CapsuleCollider>().enabled = false;
        missileRigidbody.AddForce(newMissile.transform.forward * launchForce, ForceMode.Force);

        yield return new WaitForSeconds(Random.Range(0.5f, 1));
        if (missileScript != null)
        {
            missileScript.SetTarget(target);
        }
        newMissile.GetComponent<Missile>().enabled = true;
        newMissile.GetComponent<CapsuleCollider>().enabled = true;

        RadarScanner.ships.Add(newMissile.transform.GetComponent<TeamController>());
    }
}
