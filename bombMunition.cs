using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bombMunition : MonoBehaviour
{
    public ParticleSystem explosionPS;

    public void Drop()
    {
        // Add a rigidbody to the bomb
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        Rigidbody parentrb = transform.root.GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        MeshCollider parentCollider = transform.parent.GetComponent<MeshCollider>();
        if (parentCollider != null)
        {
            Physics.IgnoreCollision(parentCollider, GetComponent<Collider>());
        }

        // Set the bomb's parent to null
        transform.parent = null;

        // Slowly lerp its z rotation to be zero
        StartCoroutine(LerpRotation());
    }

    private IEnumerator LerpRotation()
    {
        float duration = 3.0f; // Adjust the duration as needed
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0f);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final rotation is exactly zero
        transform.rotation = targetRotation;
        yield return new WaitForSeconds(1);
        transform.GetComponent<CapsuleCollider>().enabled = true;
    }
}
