using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSplash : MonoBehaviour
{
    public GameObject waterParticles;

    private void OnCollisionEnter(Collision cube)
    {
        if(cube.gameObject.tag == "Water")
        {
            Instantiate(waterParticles, transform.position, Quaternion.identity);
        }
    }
}