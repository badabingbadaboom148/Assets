using System.Collections;
using UnityEngine;

public class ballast : MonoBehaviour
{
    public Transform relativeFloater;
    private float originalY;
    public bool isFlooding;
    public float floodedAmount;
    public float floodedMaxAmount;

    private void Start()
    {
        originalY = relativeFloater.localPosition.y;
    }

    private void Update()
    {
        if (isFlooding)
        {
            floodedAmount += 5f * Time.deltaTime; // Increase floodedAmount over time
            floodedAmount = Mathf.Clamp(floodedAmount, 0f, 100f);

            float newY = Mathf.Lerp(0f, -100f, -floodedAmount / 100f);
            StartFlooding();
            Vector3 newLocalPosition = relativeFloater.localPosition;
            newLocalPosition.y = newY;

            relativeFloater.localPosition = newLocalPosition;
        }
        else
        {
            // Restore the original Y position
            Vector3 originalPosition = relativeFloater.localPosition;
            originalPosition.y = originalY;
            relativeFloater.localPosition = originalPosition;
            StopFlooding();
            floodedAmount = 0f;
        }
    }

    // Call this method to start the flooding coroutine
    public void StartFlooding()
    {
        if (!isFlooding)
        {
            isFlooding = true;
            StartCoroutine(Flood());
        }
    }

    // Call this method to stop the flooding coroutine
    public void StopFlooding()
    {
        if (isFlooding)
        {
            isFlooding = false;
        }
    }

    public IEnumerator Flood()
    {
        if (floodedAmount < floodedMaxAmount)
        {
            floodedAmount += 1f;
            yield return new WaitForSeconds(1);
        }
        else if (floodedAmount >= floodedMaxAmount)
        {
            floodedAmount = floodedMaxAmount;
        }
    }
}
