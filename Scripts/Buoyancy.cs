using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Buoyancy : MonoBehaviour
{
    public Transform[] floaters;
    public float underWaterDrag = 3f;
    public float underWaterAngularDrag = 1f;
    public float airDrag = 0f;
    public float airAngularDrag = 0.05f;
    public float floatingPower = 15f;
    public float waterHeight = 0f;
    public float range = 2f;

    Rigidbody m_RigidBody;

    int floatersUnderwater;

    bool underwater;

    void Start()
    {
        m_RigidBody = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        floatersUnderwater = 0;
        for (int i = 0; i < floaters.Length; i++)
        {
            float difference = floaters[i].transform.position.y - waterHeight;

            if (difference < 0)
            {
                m_RigidBody.AddForceAtPosition(Vector3.up * floatingPower * Mathf.Abs(difference), floaters[i].position, ForceMode.Force);
                floatersUnderwater += 1;
                if (!underwater)
                {
                    underwater = true;
                    SwitchState(true);
                }
            }
        }
        if (underwater && floatersUnderwater == 0)
        {
            underwater = false;
            SwitchState(false);
        }
    }
    void SwitchState(bool isUnderwater)
    {
        if (isUnderwater)
        {
            m_RigidBody.drag = underWaterDrag;
            m_RigidBody.angularDrag = underWaterAngularDrag;
        }
        else
        {
            m_RigidBody.drag = airDrag;
            m_RigidBody.angularDrag = airAngularDrag;
        }
    }
}