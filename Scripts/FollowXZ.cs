using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowXZ : MonoBehaviour
{
    public Transform target; // Drag and drop the GameObject you want to follow into this field in the Unity Inspector.

    private void Update()
    {
        if (target != null)
        {
            // Update the position of the image to match the position of the target GameObject.
            transform.position = target.position;
        }
    }
}