using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkBelowY : MonoBehaviour
{
    private ParticleSystem particleSystem;

    // Start is called before the first frame update
    void Start()
    {
        // Get the ParticleSystem component attached to this GameObject
        particleSystem = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the Y position is below 0
        if (transform.position.y < 0)
        {
            // Stop the ParticleSystem if it's not null
            if (particleSystem != null)
            {
                particleSystem.Stop();
            }
        }
    }
}
