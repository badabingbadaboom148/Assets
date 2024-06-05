using UnityEngine;

public class FlakTurret : MonoBehaviour
{
    [Header("Turret Settings")]
    public Transform horizontalPivot;
    public Transform verticalPivot;
    public Transform target;

    [Header("Rotation Settings")]
    public float horizontalRotationSpeed = 5f;
    public float verticalRotationSpeed = 5f;
    public float verticalPositiveRotationLimit = 60f;
    public float verticalNegativeRotationLimit = 60f;
    public float horizontalRotationLimit = 180f; // Adjust as needed

    [Header("Firing Settings")]
    public float firingAngleThreshold = 5f; // Adjust as needed
    public float fireRate = 1f;
    private float nextFireTime = 0f;
    public bool isAiming;
    public float horizontalSpread;
    public float verticalSpread;

    [Header("Projectile Settings")]
    public GameObject flakBulletPrefab;
    public Transform[] firePoints;

    [Header("Timed-Fuze Settings")]
    public float fuzeDuration = 3f; // Adjust as needed

    [Header("Velocity Settings")]
    public float bulletSpeed = 20f;

    private Vector3 targetAimPosition;
    public Vector3 predictedTargetPosition;
    public float timeOfFlight;

    void Update()
    {
        if (target != null)
        {
            // Calculate the predicted target position
            CalculatePredictedTargetPosition();

            // Calculate the rotation to the target aim position
            CalculateRotationToAimPosition();

            // Check if it's time to fire and if the turret is aimed at the target
            if (Time.time >= nextFireTime && IsAimedAtTarget())
            {
                Fire();
                // Set the next fire time
                nextFireTime = Time.time + fireRate + Random.Range(-0.25f, 0.25f);
            }
        }
    }

    void CalculateRotationToAimPosition()
    {
        // Calculate the direction to the target aim position
        Vector3 targetDirection = predictedTargetPosition - transform.position;

        // Calculate the rotation angles
        Quaternion targetRotationHorizontal = Quaternion.LookRotation(targetDirection.normalized);
        Quaternion targetRotationVertical = Quaternion.LookRotation(targetDirection.normalized);

        // Smoothly rotate the horizontal pivot towards the target aim position
        float clampedHorizontalRotation = Mathf.Clamp(targetRotationHorizontal.eulerAngles.y, -horizontalRotationLimit, horizontalRotationLimit);
        horizontalPivot.rotation = Quaternion.Slerp(horizontalPivot.rotation, Quaternion.Euler(horizontalPivot.localRotation.x, clampedHorizontalRotation, horizontalPivot.localRotation.z), horizontalRotationSpeed * Time.deltaTime);

        // Calculate the rotation angles only around the vertical axis
        float angleToVertical = Mathf.Atan2(targetDirection.y, targetDirection.magnitude) * Mathf.Rad2Deg;
        float clampedVerticalRotation = Mathf.Clamp(-angleToVertical, -verticalNegativeRotationLimit, verticalPositiveRotationLimit);

        // Apply the adjusted vertical rotation directly to the vertical pivot
        verticalPivot.localRotation = Quaternion.Euler(clampedVerticalRotation, verticalPivot.localRotation.y, verticalPivot.localRotation.z);
    }


    public void SetTargetGun(Transform targetPosition)
    {

        if (targetPosition != null)
        {
            this.target = targetPosition;
        }
        else
        {
            Debug.LogWarning("Trying to set a null target.");
        }
    }

    void CalculatePredictedTargetPosition()
    {
        if (target != null)
        {
            Vector3 aimPoint = target.position + target.GetComponent<Rigidbody>().velocity * 0.75f * timeOfFlight;

            float distance = Vector3.Distance(transform.position, aimPoint);

            // Calculate timeOfFlight, clamp it to avoid division by zero or negative values
            timeOfFlight = Mathf.Clamp(distance / bulletSpeed, 0f, 10f); // Adjust the maximum time as needed

            // Calculate the predicted position accounting for gravity
            predictedTargetPosition = aimPoint + 0 * Physics.gravity * timeOfFlight * timeOfFlight * -1;

            // Visualize the predicted target position
            Debug.DrawLine(transform.position, predictedTargetPosition, Color.blue);
        }
    }

    bool IsAimedAtTarget()
    {
        if (target != null)
        {
            // Check if the target is a missile
            Missile missile = target.GetComponent<Missile>();
            if (missile != null)
            {
                // Calculate the direction to the target
                Vector3 dirToTarget = (predictedTargetPosition - verticalPivot.position).normalized;

                // Calculate the angle between the turret's forward and the direction to the missile
                float angle = Mathf.Abs(Vector3.Angle(verticalPivot.forward, dirToTarget));

                // Return true if the angle is below the firing threshold
                return angle < firingAngleThreshold;
            }
        }

        // Return false if the target is null or not a missile
        return false;
    }

    private int currentFirePointIndex = 0;

    void Fire()
    {
        // Check if there are fire points available
        if (firePoints.Length == 0)
        {
            Debug.LogError("No fire points assigned to the turret.");
            return;
        }

        Transform currentFirePoint = firePoints[currentFirePointIndex];
        float horizontalSpreadAngle = Random.Range(-horizontalSpread, horizontalSpread);
        float verticalSpreadAngle = Random.Range(-verticalSpread, verticalSpread);
        Quaternion spreadRotation = Quaternion.Euler(verticalSpreadAngle, horizontalSpreadAngle, 0f);
        Vector3 spreadDirection = spreadRotation * currentFirePoint.forward;

        // Instantiate the flakBullet
        GameObject flakBullet = Instantiate(flakBulletPrefab, currentFirePoint.position, spreadRotation);

        // Access the flakMunition component of the instantiated bullet
        flakMunition flakMunitionTimeFuze = flakBullet.GetComponent<flakMunition>();

        // Check if the flakMunition component is not null
        if (flakMunitionTimeFuze != null)
        {
            // Set the detonation delay based on the time of flight
            flakMunitionTimeFuze.detonationDelay = timeOfFlight + Random.Range(0.25f, -0.5f);
        }
        else
        {
            Debug.LogError("flakMunition component not found on flakBullet or its children.");
        }

        // Set the bullet's forward direction based on the localized spreadDirection
        flakBullet.transform.forward = spreadDirection;

        // Access the Rigidbody component of the instantiated bullet
        Rigidbody bulletRb = flakBullet.GetComponent<Rigidbody>();

        // Check if the Rigidbody component is not null
        if (bulletRb != null)
        {
            // Set velocity based on the turret's forward direction and specified bullet speed
            bulletRb.velocity = flakBullet.transform.forward * bulletSpeed;
        }
        else
        {
            Debug.LogError("Rigidbody component not found on flakBullet or its children.");
        }

        // Increment the fire point index and wrap around if it exceeds the array length
        currentFirePointIndex = (currentFirePointIndex + 1) % firePoints.Length;
    }

    void OnDrawGizmos()
    {
        if (target != null)
        {
            // Draw a line from the turret's position to the aim position
            Gizmos.color = Color.green;
            Gizmos.DrawLine(verticalPivot.position, predictedTargetPosition);

            // Draw a sphere at the aim position
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetAimPosition, 0.5f);
        }
    }

}
