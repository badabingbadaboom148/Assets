using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 initialVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetVelocity(Vector3 targetPosition, float initialVelocity)
    {
        // Calculate the direction to the target.
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;

        // Calculate the launch angle to hit the target.
        float launchAngle = Mathf.Asin(directionToTarget.y / initialVelocity);

        // Calculate the launch velocity components.
        float horizontalVelocity = initialVelocity * Mathf.Cos(launchAngle);
        float verticalVelocity = initialVelocity * Mathf.Sin(launchAngle);

        // Set the initial velocity.
        this.initialVelocity = directionToTarget * horizontalVelocity + Vector3.up * verticalVelocity;

        // Apply the initial velocity to the rigidbody.
        rb.velocity = this.initialVelocity;
    }
}