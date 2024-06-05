using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flakTurretScanner : MonoBehaviour
{
    public enum Mode
    {
        NEAREST,
        FURTHEST
    }

    [Header("Settings")]
    [Tooltip("How often to scan in second")]
    public float IdleScanSpeed;
    public float ActiveScanSpeed;

    [Tooltip("Scanner view angle")]
    [Range(0, 360)]
    public float ViewAngle;

    [Tooltip(" Layers the scanner will detect")]
    public LayerMask Mask;

    [Tooltip("Get scanner range / or radius")]
    public float ScanRadius;

    [Tooltip("On or Off gizmos")]
    public bool ShowGizmos;

    [Tooltip("Turret Controller")]
    public FlakTurret AntiMissileGunController;

    [Tooltip("Turret modes NOTE: Only working for anti-missile gun controller")]
    public Mode TurretModes = Mode.NEAREST;

    [Tooltip("Determine if the scanner is looking for 'MissileTeamOne'")]
    public bool ScanForMissileTeamOne = true;

    public static List<Transform> targetList = new List<Transform>(); // List of targets position

    private void Start()
    {
        if (AntiMissileGunController == null)
            Debug.Log("No controller found, Please drag it into this script");
        StartCoroutine(IdleScanIteration()); // Start scan for target
    }

    IEnumerator IdleScanIteration() // repeat scanning
    {
        while (true)
        {
            yield return new WaitForSeconds(IdleScanSpeed);
            ScanForTarget();
        }
    }

    public Vector3 GetViewAngle(float angle)
    {
        // Calculate the Vector3 of the given angle for visualization
        float radiant = (angle + transform.eulerAngles.y) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radiant), 0, Mathf.Cos(radiant));
    }

    private void OnDrawGizmos()
    {
        if (!ShowGizmos) return; // Show the visualization if "ShowGizmos" is true

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, ScanRadius);
        Gizmos.DrawLine(transform.position, transform.position + GetViewAngle(ViewAngle / 2) * ScanRadius);
        Gizmos.DrawLine(transform.position, transform.position + GetViewAngle(-ViewAngle / 2) * ScanRadius);

        Gizmos.color = Color.red;
        if (targetList.Count == 0) return;
        foreach (Transform target in targetList)
        {
            if (target == null) continue;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }

    public void ScanForTarget()
    {
        targetList.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, ScanRadius, Mask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform targetPosition = targetsInViewRadius[i].transform;
            if (IsTargetValid(targetPosition))
            {
                TeamController teamController = targetPosition.GetComponent<TeamController>();
                if (teamController != null)
                {
                    if (teamController.isSpotted)
                    {
                        targetList.Add(targetPosition);
                    }
                    else if (!teamController.isSpotted)
                    {
                        targetList.Remove(targetPosition);
                    }
                }
            }
        }

        // Select target
        switch (TurretModes)
        {
            case Mode.NEAREST:
                SelectClosestTarget();
                break;

            case Mode.FURTHEST:
                SelectFurthersTarget();
                break;
        }
        targetList.RemoveAll(target => Vector3.Distance(transform.position, target.position) > ScanRadius);
    }

    private bool IsTargetValid(Transform target)
    {
        if (target == null)
            return false;

        Missile missile = target.GetComponent<Missile>();
        if (missile == null)
            return false;

        // Check if the missile is friendly or not based on the isFriendly flag
        if (ScanForMissileTeamOne && missile.tag == "MissileTeamOne")
            return true;
        else if (!ScanForMissileTeamOne && missile.tag == "MissileTeamTwo")
            return true;

        return false;
    }

    private void RemoveOutOfRangeMissiles()
    {
        for (int i = targetList.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(transform.position, targetList[i].position) > ScanRadius)
            {
                targetList.RemoveAt(i);
            }
        }
    }

    private void SelectClosestTarget()
    {
        if (targetList.Count == 0) return;

        if (targetList.Count == 1)
        {
            SetTargetGun(targetList[0]);
            return;
        }

        Transform currentTarget = null;
        float closestDistance = float.MaxValue; // Initialize to a large value

        for (int i = 0; i < targetList.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, targetList[i].position);

            if (currentTarget == null || distance < closestDistance)
            {
                currentTarget = targetList[i];
                closestDistance = distance;
            }
        }

        // Set the target after finding the closest one
        SetTargetGun(currentTarget);
    }

    private void SelectFurthersTarget()
    {
        if (targetList.Count == 0) return;

        if (targetList.Count == 1)
        {
            SetTargetGun(targetList[0]);
            return;
        }

        Transform currentTargets = null;
        float furthestDistance = 0f;
        for (int i = 0; i < targetList.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, targetList[i].position);

            if (currentTargets == null || distance > furthestDistance)
            {
                currentTargets = targetList[i];
                furthestDistance = distance;
                SetTargetGun(currentTargets);
            }
        }
    }

    private void SetTargetGun(Transform targetPosition)
    {
        AntiMissileGunController.SetTargetGun(targetPosition);
    }
}