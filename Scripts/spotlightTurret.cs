using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spotlightTurret : MonoBehaviour
{
    public Transform target;
    public float speed;

    void Update()
    {
        Vector3 targetDirection = target.position - transform.position;
        float singleStep = speed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

        Debug.DrawRay(transform.position, newDirection, Color.red);

        transform.rotation = Quaternion.LookRotation(newDirection);
    }
}
