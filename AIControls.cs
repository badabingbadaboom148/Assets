using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIControls : MonoBehaviour
{
    private RadarScanner radarScanner;
    private ShipMovementController shipMovementController;
    private airship airship;
    private TeamController teamController;
    private MasterGunController masterGunController;
    public Transform closestShip;
    public Transform closestSpottedShip;
    public LayerMask obstacleLayer;

    private Vector3 randomDirection;
    public Vector3 newDestination;

    private bool isTargetSelected = false;
    void Start()
    {
        FindClosestShip();
        radarScanner = GetComponent<RadarScanner>();
        shipMovementController = GetComponent<ShipMovementController>();
        if (transform.GetComponent<ShipMovementController>() == null)
        {
            airship = GetComponent<airship>();
        }
        teamController = GetComponent<TeamController>();
        masterGunController = GetComponent<MasterGunController>();
        ChooseDestination();
    }
    public void ChooseDestination()
    {
        // Generate a random direction within a cone
        randomDirection = Random.onUnitSphere.normalized;

        if (closestShip != null)
        {
            if (shipMovementController != null && airship == null)
            {
                newDestination = randomDirection * shipMovementController.gunMaxRange * 0.5f + closestShip.transform.position;
                newDestination = new Vector3(newDestination.x, 0, newDestination.z);
            }
            else if (airship != null && shipMovementController == null)
            {
                newDestination = randomDirection * airship.gunMaxRange * 0.5f + closestShip.transform.position;
                newDestination = new Vector3(newDestination.x, Mathf.Clamp(newDestination.y, transform.position.y - 200, transform.position.y + 200), newDestination.z);
            }
        }
        if (shipMovementController != null && airship == null)
        {
            shipMovementController.targetPosition = newDestination;
        }
        if (shipMovementController == null && airship != null)
        {
            airship.targetPosition = newDestination;
        }
    }
    private GameObject previousTarget = null;
    private void Update()
    {
        FindClosestShip();
        FindClosestSpottedShip();

        // Check if closestSpottedShip is not null
        if (closestSpottedShip != null)
        {
            // Check if the closest ship is spotted and has a RadarScanner component
            if (closestSpottedShip.GetComponent<TeamController>().isSpotted && closestSpottedShip.GetComponent<RadarScanner>() != null)
            {
                // Check if the closest spotted ship has changed
                if (closestSpottedShip != previousTarget)
                {
                    isTargetSelected = false; // Reset the flag
                    previousTarget = closestSpottedShip.gameObject; // Update the previous target
                }

                masterGunController.SetTargets(closestSpottedShip.gameObject);

                if (!isTargetSelected)
                {
                    if (Random.Range(0f, 1f) < 0.25f && closestSpottedShip != previousTarget)
                    {
                        // Set targets for gun controller and missiles
                        missileTubes missiles = transform.GetComponent<missileTubes>();
                        if (missiles != null)
                        {
                            missiles.target = closestSpottedShip.gameObject;
                            missiles.StartCoroutine(missiles.FireSalvo());
                            isTargetSelected = true;
                            previousTarget = closestSpottedShip.gameObject; // Update the previous target
                        }
                    }
                    else
                    {
                        isTargetSelected = false;
                    }
                }
            }
            else if (!closestShip.GetComponent<TeamController>().isSpotted)
            {
                masterGunController.CeaseFire();
                isTargetSelected = false;
            }
        }
        else
        {
            // If no closest spotted ship is found, reset the previous target
            previousTarget = null;
        }
        if (closestShip != null)
        {
            // Calculate the new destination
            if (shipMovementController != null || airship != null)
            {
                newDestination = randomDirection * (shipMovementController != null ? shipMovementController.gunMaxRange : airship.gunMaxRange) * 0.5f + closestShip.transform.position;
                newDestination = new Vector3(newDestination.x, Mathf.Clamp(newDestination.y, transform.position.y - 200, transform.position.y + 200), newDestination.z);
            }

            if (shipMovementController != null && airship == null)
            {
                shipMovementController.targetPosition = newDestination;
            }
            if (shipMovementController == null && airship != null)
            {
                airship.targetPosition = newDestination;
            }

            // Shoot a ray towards the new destination
            RaycastHit hit;
            if (Physics.Raycast(transform.position, newDestination - transform.position, out hit, Vector3.Distance(transform.position, newDestination), obstacleLayer))
            {
                // If the ray hits an obstacle, reroll the destination
                ChooseDestination();
            }
        }
        else
        {
            // If closestShip is not assigned, log a message or handle it accordingly
            Debug.LogWarning("closestShip is not assigned in AIControls.");
            isTargetSelected = false;
        }
    }
    void FindClosestShip()
    {
        float closestDistance = float.MaxValue;
        if (RadarScanner.ships.Count >= 1)
        {
            foreach (var ship in RadarScanner.ships)
            {
                // Skip if the ship is the same as the current team controller, friendly, or doesn't have a RadarScanner component
                if (ship == teamController || GetComponent<TeamController>().isFriendly == ship.isFriendly || ship.transform.GetComponent<Missile>() != null)
                {
                    continue;
                }
                // Calculate the distance to the ship
                float distance = Vector3.Distance(transform.position, ship.transform.position);

                // Update the closest ship if this ship is closer
                if (distance < closestDistance)
                {
                    closestShip = ship.transform;
                    closestDistance = distance;
                }
            }
        }
    }
    void FindClosestSpottedShip()
    {
        float closestDistance = float.MaxValue;
        foreach (TeamController ship in RadarScanner.ships)
        {
            if (ship == teamController || ship.isFriendly == teamController.isFriendly || ship.GetComponent<RadarScanner>() == null || !ship.isSpotted)
            {
                continue;
            }
            float distance = Vector3.Distance(transform.position, ship.transform.position);

            // Update the closest ship if this ship is closer
            if (distance < closestDistance)
            {
                closestSpottedShip = ship.transform;
                closestDistance = distance;
            }
        }
    }
}
