using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{

    private Rigidbody rigidbody;

    [SerializeField] private float ForwardForce = 5;
    [SerializeField] private float TurningTorque = .1f;
    [SerializeField] Vector3 m_EulerAngularVelocity;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        m_EulerAngularVelocity = new Vector3(0, 30, 0);

        //Forward Force
        if (Input.GetKey(KeyCode.W))
        {
            rigidbody.AddRelativeForce(Vector3.forward * ForwardForce, ForceMode.Acceleration);
        }

        //if (Input.GetKey(KeyCode.S))
        //{
            //rigidbody.AddRelativeForce(Vector3.forward * -ForwardForce, ForceMode.Acceleration);
        //}

        if (Input.GetKey(KeyCode.D))
        {
            rigidbody.AddRelativeTorque(Vector3.up * TurningTorque, ForceMode.Acceleration);
        }

        if (Input.GetKey(KeyCode.A))
        {
            rigidbody.AddRelativeTorque(Vector3.down * TurningTorque, ForceMode.Acceleration);
        }
    }
}