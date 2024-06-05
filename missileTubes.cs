// missileTubes.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class missileTubes : MonoBehaviour
{
    public GameObject target;
    public Missile missilePrefab;
    public Transform[] tubes;
    public float salvoSize;
    public float salvoCooldown;
    public float timeSince = 29f;

    private int currentIndex = 0;
    private bool isFiring = false;
    private void Start()
    {
        timeSince = 30;
    }

    void Update()
    {
        timeSince += Time.deltaTime;
        timeSince = Mathf.Clamp(timeSince, 0, 30.1f);
    }

    public IEnumerator FireSalvo()
    {
        if (timeSince >= salvoCooldown)
        {
            isFiring = true;
            Debug.Log("fire salvo");
            for (int i = 0; i < salvoSize; i++)
            {
                if (currentIndex < tubes.Length)
                {
                    tubeSpawnMissile tubeSpawn = tubes[currentIndex].GetComponent<tubeSpawnMissile>();
                    tubeSpawn.SetTarget(target); // Set the target before firing
                    tubeSpawn.Fire();
                    tubeSpawn.missilePrefab = missilePrefab.transform.gameObject;
                    currentIndex++;
                    timeSince = 0;
                }
                else
                {
                    break; // Break the loop if we have fired from all tubes
                }
            }
        }
        else
        {
            yield break;
        }
        isFiring = false;
    }
}
