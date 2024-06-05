using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GT2
{
    /// <summary>
    /// Checks for ships within a certain range
    /// </summary>
    public class navalGunController : MonoBehaviour
    {
        [Header("Rotations")]

        [Tooltip("Transform of the turret's azimuthal rotations.")]
        [SerializeField] private Transform turretBase = null;

        [Tooltip("Transform of the turret's elevation rotations.")]
        [SerializeField] private Transform barrels = null;

        [Header("Elevation")]
        [Tooltip("Speed at which the turret's guns elevate up and down.")]
        public float ElevationSpeed = 30f;

        [Tooltip("Highest upwards elevation the turret's barrels can aim.")]
        public float MaxElevation = 60f;

        [Tooltip("Lowest downwards elevation the turret's barrels can aim.")]
        public float MaxDepression = 5f;

        [Header("Traverse")]

        [Tooltip("Speed at which the turret can rotate left/right.")]
        public float TraverseSpeed = 60f;

        [Tooltip("When true, the turret can only rotate horizontally with the given limits.")]
        [SerializeField] private bool hasLimitedTraverse = false;
        [Range(0, 179)] public float LeftLimit = 120f;
        [Range(0, 179)] public float RightLimit = 120f;

        [Header("Behavior")]

        [Tooltip("When idle, the turret does not aim at anything and simply points forwards.")]
        public bool IsIdle = false;

        [Tooltip("Position the turret will aim at when not idle.")]
        public Vector3 AimPosition = Vector3.zero;

        [Tooltip("When the turret is within this many degrees of the target, it is considered aimed.")]
        [SerializeField] private float aimedThreshold = 5f;
        private float limitedTraverseAngle = 0f;

        [Header("Target")]
        [Tooltip("Target")]
        public GameObject target;
        public Vector3 targetPosition;

        [Header("Shooting")]
        public GameObject projectilePrefab;
        public Transform[] firePoints;
        public float fireRate = 2f;
        private float fireCooldown = 0f;
        public float maxRange;
        public float projectileSpeed;
        public ParticleSystem cannonFlash;
        public float recoilForce = 1000f;
        public float verticalSpreadPositive;
        public float verticalSpreadNegative;
        public float horizontalSpreadPositive;
        public float horizontalSpreadNegative;

        public bool DrawDebugRay = true;
        public bool DrawDebugArcs = false;
        public GameObject referenceObject;
        public float addY;
        float gravity = 9.81f;
        private float launchAngleDegrees;

        private float angleToTarget = 0f;
        private float elevation = 0f;

        private bool hasBarrels = false;

        private bool isAimed = false;
        private bool isBaseAtRest = false;
        private bool isBarrelAtRest = false;

        public bool HasLimitedTraverse => hasLimitedTraverse;
        public bool IsTurretAtRest => isBarrelAtRest && isBaseAtRest;
        public bool IsAimed => isAimed;
        public float AngleToTarget => IsIdle ? 999f : angleToTarget;
        public float[] fireCooldowns; // Individual cooldowns for each firepoint
        private Vector3 previousPosition;
        private List<Transform> shipChildren = new List<Transform>();
        private Transform[] childObjects;
        private int currentShipChildIndex = -1;
        private Transform lastTarget;

        private Camera camera;

        public float caliber;
        public float cameraShakeIntensity = 0.25f;
        private void Start()
        {
            hasBarrels = barrels != null;
            if (turretBase == null)
                Debug.LogError(name + ": TurretAim requires an assigned TurretBase!");

            // Initialize the fireCooldowns array
            fireCooldowns = new float[firePoints.Length];
            camera = Camera.main;
        }

        private void Update()
        {
            if (target != null)
            {
                IsIdle = false;
                targetPosition = target.transform.position;
                AimPosition = CalculatePredictedAimPosition(targetPosition);

                float distanceToTarget = Vector3.Distance(transform.position, AimPosition);
                if (IsIdle || distanceToTarget > maxRange)
                {
                    if (!IsTurretAtRest)
                        RotateTurretToIdle();
                    isAimed = false;
                }
                else
                {
                    RotateBaseToFaceTarget(AimPosition);

                    if (hasBarrels)
                        RotateBarrelsToFaceTarget(AimPosition);

                    angleToTarget = GetTurretAngleToTarget(AimPosition);

                    isAimed = angleToTarget < aimedThreshold;

                    isBarrelAtRest = false;
                    isBaseAtRest = false;
                }

                // Update cooldowns for each fire point
                for (int i = 0; i < firePoints.Length; i++)
                {
                    fireCooldowns[i] -= Time.deltaTime;
                }

                // Check if the turret is ready to fire
                if (!IsIdle && isAimed)
                {
                    for (int i = 0; i < firePoints.Length; i++)
                    {
                        // Check if the current firepoint can fire
                        if (fireCooldowns[i] <= 0f)
                        {
                            FireProjectile(firePoints[i]);
                            InitializeTarget(target); // Initialize target here
                            // Reset the cooldown for the current firepoint
                            fireCooldowns[i] = fireRate + Random.Range(-0.1f, 0.1f); // Adjust as needed
                            Mathf.Clamp(fireRate, -1f, 1f);
                        }
                    }
                }

            }
            else if (target == null)
            {
                IsIdle = true;
            }
        }
        private void InitializeTarget(GameObject newTarget)
        {
            // Clear and refill shipChildren and childObjects for the new target
            shipChildren.Clear();

            if (newTarget != null && newTarget.transform.childCount > 0)
            {
                childObjects = newTarget.GetComponentsInChildren<Transform>();

                foreach (Transform child in childObjects)
                {
                    if (child.CompareTag("Ship"))
                    {
                        shipChildren.Add(child);
                    }
                }
            }
            lastTarget = newTarget.transform;
            currentShipChildIndex = -1;
        }
        private void FireProjectile(Transform firePoint)
        {
            // Check if the target has changed
            if (target != null && target.transform != lastTarget)
            {
                lastTarget = target.transform;  // Update lastTarget to current target

                // Clear and refill shipChildren and childObjects for the new target
                shipChildren.Clear();
                childObjects = target.GetComponentsInChildren<Transform>();

                foreach (Transform child in childObjects)
                {
                    if (child.CompareTag("Ship"))
                    {
                        shipChildren.Add(child);
                    }
                }
            }

            // Check if there are ship children available
            if (shipChildren.Count > 0)
            {
                // Check if it's the first time or if it's time to select a new target
                if (currentShipChildIndex == -1 || target.transform == lastTarget)
                {
                    // Select a random ship child
                    currentShipChildIndex = Random.Range(0, shipChildren.Count);
                }

                Transform newTarget = shipChildren[currentShipChildIndex];
                target = newTarget.gameObject;
                IsIdle = false;

                // Increment the index for the next iteration
                currentShipChildIndex++;
            }
            else
            {

            }

            float horizontalSpreadAngle = Random.Range(horizontalSpreadNegative, horizontalSpreadPositive);
            float verticalSpreadAngle = Random.Range(verticalSpreadNegative, verticalSpreadPositive);
            Quaternion spreadRotation = Quaternion.Euler(verticalSpreadAngle, horizontalSpreadAngle, 0f);
            Vector3 spreadDirection = spreadRotation * firePoint.forward;

            GameObject newProjectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(spreadDirection));
            Rigidbody projectileRb = newProjectile.GetComponent<Rigidbody>();
            newProjectile.GetComponent<CannonShell>().projectileSpeed = projectileSpeed;
            Instantiate(cannonFlash, firePoint.position, firePoint.rotation);
            cannonFlash.Play();

            if (projectileRb != null)
            {
                projectileRb.velocity = spreadDirection * projectileSpeed;
            }

            // Additional logic if needed
        }

        private IEnumerator SlideFirePointsBack(Transform[] firePoints, Vector3 recoilDirection, float slideDuration)
        {
            Vector3[] originalPositions = new Vector3[firePoints.Length];

            // Store the original positions of firePoints
            for (int i = 0; i < firePoints.Length; i++)
            {
                originalPositions[i] = firePoints[i].transform.localPosition;
            }

            float startTime = Time.time;
            float progress = 0f;

            while (progress < 1f)
            {
                progress = Mathf.Clamp01((Time.time - startTime) / slideDuration);

                // Apply the slide to each firePoint using SmoothStep for a smoother interpolation
                for (int i = 0; i < firePoints.Length; i++)
                {
                    Vector3 newPosition = Vector3.Lerp(originalPositions[i], Vector3.zero, Mathf.SmoothStep(0f, 1f, progress));
                    firePoints[i].localPosition = newPosition;
                }

                // Apply the recoil force gradually during the slide
                Rigidbody shipRigidbody = turretBase.GetComponent<Rigidbody>();
                if (shipRigidbody != null)
                {
                    float gradualRecoilForce = recoilForce * (1 - progress); // Gradual decrease in recoil force
                    shipRigidbody.AddForce(recoilDirection * gradualRecoilForce, ForceMode.Impulse);
                }

                yield return null;
            }

            // Ensure the firePoints smoothly reach their original positions
            for (int i = 0; i < firePoints.Length; i++)
            {
                firePoints[i].localPosition = originalPositions[i];
            }
        }

        private Vector3 CalculatePredictedAimPosition(Vector3 currentPosition)
        {
            float horizontalPredictionMultiplier = 1f;
            float verticalPredictionMultiplier = 6f;
            // Calculate horizontal prediction time
            float horizontalTimeToReach = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(currentPosition.x, currentPosition.z)) / projectileSpeed;

            // Fine-tune the horizontal prediction time
            float horizontalPredictionTime = horizontalTimeToReach * horizontalPredictionMultiplier;

            // Calculate angle between turret's forward direction and vector to target
            Vector3 toTarget = (currentPosition - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, toTarget);

            // Adjust vertical prediction multiplier based on angle to target
            float adjustedVerticalMultiplier = Mathf.Lerp(1f, verticalPredictionMultiplier, Mathf.InverseLerp(0f, 90f, angleToTarget));

            Vector3 targetVelocity = (target.transform.position - previousPosition) / Time.deltaTime;
            float verticalVelocity = Vector3.Dot(targetVelocity, transform.up);

            float verticalDisplacement = 0.5f * Physics.gravity.magnitude * horizontalTimeToReach * horizontalTimeToReach + verticalVelocity * horizontalTimeToReach;

            // Calculate vertical prediction time
            float verticalTimeToReach = Mathf.Sqrt(2f * verticalDisplacement / Physics.gravity.magnitude) * adjustedVerticalMultiplier;

            // Calculate predicted position
            Vector3 horizontalVelocity = (currentPosition - previousPosition) / Time.deltaTime;
            Vector3 predictedHorizontalPosition = currentPosition + horizontalVelocity * horizontalPredictionTime;
            Vector3 predictedPosition = predictedHorizontalPosition + Vector3.up * verticalDisplacement;
            previousPosition = currentPosition;
            return predictedPosition;
        }
        private float GetTurretAngleToTarget(Vector3 targetPosition)
        {
            float angle = 999f;

            if (hasBarrels)
            {
                angle = Vector3.Angle(targetPosition - barrels.position, barrels.forward);
            }
            else
            {
                Vector3 flattenedTarget = Vector3.ProjectOnPlane(
                    targetPosition - turretBase.position,
                    turretBase.up);

                angle = Vector3.Angle(
                    flattenedTarget - turretBase.position,
                    turretBase.forward);
            }

            return angle;
        }
        private void RotateTurretToIdle()
        {
            if (hasLimitedTraverse)
            {
                limitedTraverseAngle = Mathf.MoveTowards(
                    limitedTraverseAngle, 0f,
                    TraverseSpeed * Time.deltaTime);

                if (Mathf.Abs(limitedTraverseAngle) > Mathf.Epsilon)
                    turretBase.localEulerAngles = Vector3.up * limitedTraverseAngle;
                else
                    isBaseAtRest = true;
            }
            else
            {
                turretBase.rotation = Quaternion.RotateTowards(
                    turretBase.rotation,
                    transform.rotation,
                    TraverseSpeed * Time.deltaTime);

                isBaseAtRest = Mathf.Abs(turretBase.localEulerAngles.y) < Mathf.Epsilon;
            }

            if (hasBarrels)
            {
                elevation = Mathf.MoveTowards(elevation, 0f, ElevationSpeed * Time.deltaTime);
                if (Mathf.Abs(elevation) > Mathf.Epsilon)
                    barrels.localEulerAngles = Vector3.right * -elevation;
                else
                    isBarrelAtRest = true;
            }
            else
                isBarrelAtRest = true;
        }

        private void RotateBarrelsToFaceTarget(Vector3 targetPosition)
        {
            Vector3 localTargetPos = turretBase.InverseTransformDirection(targetPosition - barrels.position);
            Vector3 flattenedVecForBarrels = Vector3.ProjectOnPlane(localTargetPos, Vector3.up);

            float targetElevation = Vector3.Angle(flattenedVecForBarrels, localTargetPos);
            targetElevation *= Mathf.Sign(localTargetPos.y);

            targetElevation = Mathf.Clamp(targetElevation, -MaxDepression, MaxElevation);
            elevation = Mathf.MoveTowards(elevation, targetElevation, ElevationSpeed * Time.deltaTime);

            if (Mathf.Abs(elevation) > Mathf.Epsilon)
                barrels.localEulerAngles = Vector3.right * -elevation;
        }

        private void RotateBaseToFaceTarget(Vector3 targetPosition)
        {
            Vector3 turretUp = transform.up;
            Vector3 vecToTarget = targetPosition - turretBase.position;
            Vector3 flattenedVecForBase = Vector3.ProjectOnPlane(vecToTarget, turretUp);

            if (hasLimitedTraverse)
            {
                Vector3 turretForward = transform.forward;
                float targetTraverse = Vector3.SignedAngle(turretForward, flattenedVecForBase, turretUp);
                targetTraverse = Mathf.Clamp(targetTraverse, -LeftLimit, RightLimit);
                limitedTraverseAngle = Mathf.MoveTowards(
                    limitedTraverseAngle,
                    targetTraverse,
                    TraverseSpeed * Time.deltaTime);

                if (Mathf.Abs(limitedTraverseAngle) > Mathf.Epsilon)
                    turretBase.localEulerAngles = Vector3.up * limitedTraverseAngle;
            }
            else
            {
                turretBase.rotation = Quaternion.RotateTowards(
                    Quaternion.LookRotation(turretBase.forward, turretUp),
                    Quaternion.LookRotation(flattenedVecForBase, turretUp),
                    TraverseSpeed * Time.deltaTime);
            }
        }

#if UNITY_EDITOR
        // This should probably go in an Editor script, but dealing with Editor scripts
        // is a pain in the butt so I'd rather not.
        private void OnDrawGizmosSelected()
        {
            if (!DrawDebugArcs)
                return;

            if (turretBase != null)
            {
                const float kArcSize = 10f;
                Color colorTraverse = new Color(1f, .5f, .5f, .1f);
                Color colorElevation = new Color(.5f, 1f, .5f, .1f);
                Color colorDepression = new Color(.5f, .5f, 1f, .1f);

                Transform arcRoot = barrels != null ? barrels : turretBase;

                UnityEditor.Handles.color = colorTraverse;
                if (hasLimitedTraverse)
                {
                    UnityEditor.Handles.DrawSolidArc(
                        arcRoot.position, turretBase.up,
                        transform.forward, RightLimit,
                        kArcSize);
                    UnityEditor.Handles.DrawSolidArc(
                        arcRoot.position, turretBase.up,
                        transform.forward, -LeftLimit,
                        kArcSize);
                }
                else
                {
                    UnityEditor.Handles.DrawSolidArc(
                        arcRoot.position, turretBase.up,
                        transform.forward, 360f,
                        kArcSize);
                }

                if (barrels != null)
                {
                    UnityEditor.Handles.color = colorElevation;
                    UnityEditor.Handles.DrawSolidArc(
                        barrels.position, barrels.right,
                        turretBase.forward, -MaxElevation,
                        kArcSize);

                    UnityEditor.Handles.color = colorDepression;
                    UnityEditor.Handles.DrawSolidArc(
                        barrels.position, barrels.right,
                        turretBase.forward, MaxDepression,
                        kArcSize);
                }
            }
        }
#endif
    }
}
