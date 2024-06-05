using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RadarScanner : MonoBehaviour
{
    public float radarRange = 30;
    public float radarPower;
    public float radarSensitivity = 1;

    public LayerMask obstacleLayer;

    public GameObject[] radarPanels;

    private TeamController myTeamController;

    public static List<TeamController> ships = new List<TeamController>();

    private HashSet<Transform> addedShips = new HashSet<Transform>();
    private void Start()
    {
        Missile.OnMissileDestroyed += HandleMissileDestroyed;
    }
    private void FixedUpdate()
    {
        foreach (var ship in ships)
        {
            if (ship == null)
            {
                ships.Remove(ship);
                continue;
            }
            TeamController teamController = ship.GetComponent<TeamController>();
            if (teamController.isFriendly == transform.GetComponent<TeamController>().isFriendly)
            {
                continue;
            }

            Vector3 direction = ship.transform.position - transform.position;
            float distance = Vector3.Distance(transform.position, ship.transform.position);

            float modifiedRadarPower = (radarPower / (distance * distance)) * teamController.surfaceArea;
            if (modifiedRadarPower > radarSensitivity)
            {
                if (!Physics.Raycast(transform.position, direction, distance, obstacleLayer))
                {
                    Debug.DrawLine(transform.position, ship.transform.position, Color.green);
                    if (ship.GetComponent<Missile>() != null && Vector3.Distance(transform.position, ship.transform.position) < 5000)
                    {
                        if (!addedShips.Contains(ship.transform))
                        {
                            teamController.AddSpottedShip(transform);
                            addedShips.Add(ship.transform);
                        }
                    }
                    if (Mathf.Abs(ship.transform.position.y - transform.position.y) < 75 || Vector3.Distance(transform.position, ship.transform.position) < 5 * ship.GetComponent<TeamController>().surfaceArea)
                    {
                        if (!addedShips.Contains(ship.transform))
                        {
                            teamController.AddSpottedShip(transform);
                            addedShips.Add(ship.transform);
                        }
                    }
                    else
                    {
                        teamController.RemoveSpottedShip(transform);
                        addedShips.Remove(ship.transform); // Remove the ship from addedShips HashSet if it fails the condition
                        Debug.DrawLine(transform.position, ship.transform.position, Color.red);
                    }
                }
                else
                {
                    teamController.RemoveSpottedShip(transform);
                    addedShips.Remove(ship.transform); // Remove the ship from addedShips HashSet if it fails the condition
                    Debug.DrawLine(transform.position, ship.transform.position, Color.red);
                }
            }
            else
            {
                teamController.RemoveSpottedShip(transform);
                addedShips.Remove(ship.transform); // Assuming you want to remove it if it's no longer valid.
                Debug.DrawLine(transform.position, ship.transform.position, Color.red);
            }
        }
    }
    private void HandleMissileDestroyed(Missile missile)
    {
        ships.Remove(missile.transform.GetComponent<TeamController>());
    }
}