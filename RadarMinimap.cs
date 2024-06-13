using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class RadarMinimap : MonoBehaviour
{
    public RawImage trackIcon;
    public Texture friendlyShipTexture;
    public Texture enemyShipTexture;
    public Texture missileTexture;

    private Dictionary<TeamController, RawImage> shipIcons = new Dictionary<TeamController, RawImage>();
    private Dictionary<TeamController, RectTransform> shipVelocityVectors = new Dictionary<TeamController, RectTransform>();
    private HashSet<Missile> trackedMissiles = new HashSet<Missile>();
    private HashSet<TeamController> trackedShips = new HashSet<TeamController>();
    private bool isTracked = false;

    private bool isFullscreen = false;
    private RawImage rawImage;
    private RectTransform rectTransform;
    private Vector2 originalSize;
    private Vector2 originalPosition;
    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalPivot;
    private Vector3 iconOriginalScale;
    private Dictionary<RawImage, bool> iconExpandedStates = new Dictionary<RawImage, bool>();

    private void Start()
    {
        foreach (TeamController ship in RadarScanner.ships)
        {
            RawImage shipIcon = Instantiate(trackIcon, transform.position, transform.rotation);
            shipIcon.transform.SetParent(transform);

            Transform velocityInfo = shipIcon.transform.Find("velocity info");
            velocityInfo.gameObject.SetActive(false);
            Transform headingInfo = shipIcon.transform.Find("heading info");
            headingInfo.gameObject.SetActive(false);
            RectTransform velocityVector = shipIcon.transform.Find("velocity vector").GetComponent<RectTransform>();
            RawImage vector = velocityVector.Find("vector").GetComponent<RawImage>();
            vector.color = ship.isFriendly ? Color.green : Color.red;
            shipVelocityVectors[ship] = velocityVector;

            EventTrigger trigger = shipIcon.gameObject.AddComponent<EventTrigger>();

            // Add pointer enter event
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((eventData) => { StartCoroutine(OnIconHover(shipIcon, true)); });
            trigger.triggers.Add(entryEnter);

            // Add pointer exit event
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((eventData) => { StartCoroutine(OnIconHover(shipIcon, false)); });
            trigger.triggers.Add(entryExit);

            if (ship.isFriendly)
            {
                shipIcon.texture = friendlyShipTexture;
            }
            else
            {
                shipIcon.texture = enemyShipTexture;
            }

            shipIcons[ship] = shipIcon;
            iconExpandedStates[shipIcon] = false; // Initialize the expanded state
            iconOriginalScale = shipIcon.transform.localScale;
        }
        Missile.OnMissileDestroyed += HandleMissileDestroyed;
        TeamController.OnShipDestroyed += HandleShipDestroyed;

        rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();

        // Store the original size, position, anchor, and pivot
        originalSize = rectTransform.sizeDelta;
        originalPosition = rectTransform.anchoredPosition;
        originalAnchorMin = rectTransform.anchorMin;
        originalAnchorMax = rectTransform.anchorMax;
        originalPivot = rectTransform.pivot;
    }

    private void OnDestroy()
    {
        Missile.OnMissileDestroyed -= HandleMissileDestroyed;
        TeamController.OnShipDestroyed -= HandleShipDestroyed;
    }

    private void Update()
    {
        foreach (TeamController ship in RadarScanner.ships)
        {
            Missile missile = ship.transform.GetComponent<Missile>();
            if (missile != null && !trackedMissiles.Contains(missile))
            {
                RawImage missileIcon = Instantiate(trackIcon, transform);
                missileIcon.texture = missileTexture;
                missileIcon.color = ship.isFriendly ? Color.green : Color.red;
                shipIcons[ship] = missileIcon;
                trackedMissiles.Add(missile);
                RectTransform velocityVector = missileIcon.transform.Find("velocity vector").GetComponent<RectTransform>();
                RawImage vector = velocityVector.Find("vector").GetComponent<RawImage>();
                vector.color = ship.isFriendly ? Color.green : Color.red;
                shipVelocityVectors[ship] = velocityVector;
                if (trackedMissiles.Contains(missile))
                {
                    RectTransform missileVector = shipVelocityVectors[ship];
                    float headingY = ship.transform.rotation.eulerAngles.y;
                    missileVector.rotation = Quaternion.Euler(0, 0, -headingY);
                }
            }
        }

        foreach (var entry in shipIcons)
        {
            TeamController ship = entry.Key;
            RawImage shipIcon = entry.Value;

            // Update the position of the RawImage based on the ship's position
            Vector3 shipPosition = ship.transform.position;
            Vector2 minimapPosition = ConvertToMinimapPosition(shipPosition);
            shipIcon.rectTransform.anchoredPosition = minimapPosition;

            // Update altitude text
            Transform altitudeInfo = shipIcon.transform.Find("altitudeText");
            if (altitudeInfo != null)
            {
                float altitude = ship.transform.position.y;
               if (altitudeInfo.GetComponent<Text>() != null)
                {
                    altitudeInfo.GetComponent<Text>().text = altitude.ToString("F0");
                }
            }

            // Handle visibility based on spotted status
            shipIcon.gameObject.SetActive(ship.isFriendly || ship.isSpotted);

            // Update the velocity vector's rotation
            if (shipVelocityVectors.ContainsKey(ship))
            {
                RectTransform velocityVector = shipVelocityVectors[ship];
                float headingY = ship.transform.rotation.eulerAngles.y;
                velocityVector.rotation = Quaternion.Euler(0, 0, -headingY);
            }
        }
    }
    private void HandleMissileDestroyed(Missile missile)
    {
        TeamController shipToRemove = null;

        foreach (var entry in shipIcons)
        {
            TeamController ship = entry.Key;
            if (ship.transform.GetComponent<Missile>() == missile)
            {
                shipToRemove = ship;
                Destroy(entry.Value.gameObject); // Remove the RawImage
                break;
            }
        }

        if (shipToRemove != null)
        {
            shipIcons.Remove(shipToRemove);
            trackedMissiles.Remove(missile);
        }
    }

    private void HandleShipDestroyed(TeamController teamController)
    {
        TeamController shipToRemove = null;

        foreach (var entry in shipIcons)
        {
            TeamController ship = entry.Key;
            if (ship.transform.GetComponent<TeamController>() == teamController)
            {
                shipToRemove = ship;
                Destroy(entry.Value.gameObject); // Remove the RawImage
                break;
            }
        }

        if (shipToRemove != null)
        {
            shipIcons.Remove(shipToRemove);
            trackedShips.Remove(teamController);
        }
    }

    private Vector2 ConvertToMinimapPosition(Vector3 worldPosition)
    {
        float minimapScale = isFullscreen ? 0.08f : 0.04f;
        return new Vector2(worldPosition.x * minimapScale, worldPosition.z * minimapScale);
    }

    public void RadarMinimapFullscreen()
    {
        StopAllCoroutines(); // Stop any ongoing transitions
        StartCoroutine(SmoothTransition(!isFullscreen));
        isFullscreen = !isFullscreen;
    }

    private IEnumerator SmoothTransition(bool toFullscreen)
    {
        float duration = 0.1f; // Duration of the transition
        float elapsed = 0f;

        // Start values
        Vector2 startSize = rectTransform.sizeDelta;
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 startAnchorMin = rectTransform.anchorMin;
        Vector2 startAnchorMax = rectTransform.anchorMax;
        Vector2 startPivot = rectTransform.pivot;

        // Target values
        Vector2 targetSize = toFullscreen ? new Vector2(originalSize.x * 2, originalSize.y * 2) : originalSize;
        Vector2 targetPosition = toFullscreen ? Vector2.zero : originalPosition;
        Vector2 targetAnchorMin = toFullscreen ? new Vector2(0.5f, 0.5f) : originalAnchorMin;
        Vector2 targetAnchorMax = toFullscreen ? new Vector2(0.5f, 0.5f) : originalAnchorMax;
        Vector2 targetPivot = toFullscreen ? new Vector2(0.5f, 0.5f) : originalPivot;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Interpolate values
            rectTransform.sizeDelta = Vector2.Lerp(startSize, targetSize, t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            rectTransform.anchorMin = Vector2.Lerp(startAnchorMin, targetAnchorMin, t);
            rectTransform.anchorMax = Vector2.Lerp(startAnchorMax, targetAnchorMax, t);
            rectTransform.pivot = Vector2.Lerp(startPivot, targetPivot, t);

            yield return null;
        }

        // Ensure the final values are set
        rectTransform.sizeDelta = targetSize;
        rectTransform.anchoredPosition = targetPosition;
        rectTransform.anchorMin = targetAnchorMin;
        rectTransform.anchorMax = targetAnchorMax;
        rectTransform.pivot = targetPivot;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        RadarMinimapFullscreen();
    }

    private IEnumerator OnIconHover(RawImage icon, bool isHovering)
    {
        if (isFullscreen)
        {
            // Check if the icon is already in the target state
            if (iconExpandedStates[icon] == isHovering)
                yield break;

            float duration = 0.1f; // Duration of the transition
            float elapsed = 0f;
            Vector3 targetScale = isHovering ? iconOriginalScale * 1.5f : iconOriginalScale;

            // Retrieve the ship associated with the icon
            TeamController associatedShip = null;
            foreach (var entry in shipIcons)
            {
                if (entry.Value == icon)
                {
                    associatedShip = entry.Key;
                    break;
                }
            }

            if (associatedShip == null)
            {
                Debug.LogError("No associated ship found for the hovered icon.");
                yield break;
            }

            if (isHovering)
            {
                Transform velocityInfo = icon.transform.Find("velocity info");
                if (velocityInfo != null)
                {
                    velocityInfo.gameObject.SetActive(true);
                }

                Transform headingInfo = icon.transform.Find("heading info");
                if (headingInfo != null)
                {
                    headingInfo.gameObject.SetActive(true);

                    // Use the rotation of the associated ship
                    float headingY = Mathf.Abs(associatedShip.transform.rotation.eulerAngles.y);
                    var textMeshPro = headingInfo.GetComponent<Text>();
                    if (textMeshPro != null)
                    {
                        textMeshPro.text = "HDG: " + headingY.ToString("F0");
                    }
                }

                Transform classInfo = icon.transform.Find("ship class");
                if (classInfo != null)
                {
                    classInfo.gameObject.SetActive(true);

                    // Use the rotation of the associated ship
                    float headingY = Mathf.Abs(associatedShip.transform.rotation.eulerAngles.y);
                    var textMeshPro = classInfo.GetComponent<Text>();
                    if (textMeshPro != null)
                    {
                        textMeshPro.text = associatedShip.GetComponent<TeamController>().ShipClassName;
                    }
                }
            }
            else
            {
                Transform velocityInfo = icon.transform.Find("velocity info");
                if (velocityInfo != null)
                {
                    velocityInfo.gameObject.SetActive(false);
                }

                Transform headingInfo = icon.transform.Find("heading info");
                if (headingInfo != null)
                {
                    headingInfo.gameObject.SetActive(false);
                }

                Transform classInfo = icon.transform.Find("ship class");
                if (classInfo != null)
                {
                    classInfo.gameObject.SetActive(false);
                }
                icon.transform.localScale = iconOriginalScale;
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                icon.transform.localScale = Vector3.Lerp(iconOriginalScale, targetScale, t);

                yield return null;
            }

            icon.transform.localScale = targetScale;

            // Update the expanded state
            iconExpandedStates[icon] = isHovering;
        }
    }
}
