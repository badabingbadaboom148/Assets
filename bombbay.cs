using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bombbay : MonoBehaviour
{
    public Transform leftDoor;
    public Transform rightDoor;
    public Transform target;
    public bombMunition[] bombs;
    public float doorOpenThreshold;
    public float bombDropThreshold;

    private float doorSwingSpeed = 10f;
    public bool doorsOpen;
    public bool doorsClosed;
    public bool canDropBombs;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            if (Mathf.Abs(target.position.x - transform.position.x) <= doorOpenThreshold)
            {
                if (!doorsOpen)
                {
                    doorOpen();
                }
                if (canDropBombs && Mathf.Abs(target.position.x - transform.position.x) <= bombDropThreshold)
                {
                    RaycastHit hit;
                    Physics.Raycast(transform.position, Vector3.down, out hit);
                    if (hit.transform == target.transform)
                    {
                        StartCoroutine(DropBomb());
                    }
                }
            }
            if (Mathf.Abs(target.position.x - transform.position.x) > doorOpenThreshold)
            {
                if (!doorsClosed)
                {
                    doorClose();
                }
            }
        }
    }
    private void doorOpen()
    {
        // Rotate the doors
        leftDoor.Rotate(Vector3.up, doorSwingSpeed * Time.deltaTime);
        rightDoor.Rotate(Vector3.up, -doorSwingSpeed * Time.deltaTime);

        // Get the current rotation angle of the left door
        float currentAngle = leftDoor.localEulerAngles.y;

        if (currentAngle > 0)
        {
            doorsClosed = false;
        }
        if (currentAngle >= 90f)
        {
            // Doors have fully opened
            doorsOpen = true;
            canDropBombs = true;
        }
    }

    private void doorClose()
    {
        // Rotate the doors
        leftDoor.Rotate(Vector3.up, -doorSwingSpeed * Time.deltaTime);
        rightDoor.Rotate(Vector3.up, doorSwingSpeed * Time.deltaTime);
        float angle = leftDoor.localEulerAngles.y;
        Debug.Log("angle: " + angle);
        if (angle < 90)
        {
            // Doors have fully closed
            doorsOpen = false;
            canDropBombs = false;
        }
        if (angle <= 1f)
        {
            doorsClosed = true;
        }
        else
        {
            // Doors are still closing
            doorsClosed = false;
        }
    }
    IEnumerator DropBomb()
    {
        while (true)
        {
            foreach (var bomb in bombs)
            {
                bomb.Drop();
                Debug.Log("Dropped " + bomb.transform.name);
                yield return new WaitForSeconds(0.25f);
            }
        }
    }
}