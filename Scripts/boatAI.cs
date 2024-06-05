using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boatAI : MonoBehaviour
{
    public float topSpeed;
    [Range(-50, 100)]
    public float desiredSpeed;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float newDesiredSpeed = desiredSpeed / 100;
        float currentSpeed = newDesiredSpeed * topSpeed;
        rb.AddForce(transform.forward * currentSpeed * rb.mass);
    }
}
