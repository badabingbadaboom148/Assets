using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipOverviewButtons : MonoBehaviour
{
    private static ShipOverviewButtons _Instance;
    public Button shipButtonTemplate;
    public static ShipOverviewButtons Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<ShipOverviewButtons>();
                if (_Instance == null)
                {
                    _Instance = new GameObject().AddComponent<ShipOverviewButtons>();
                }
            }
            return _Instance;
        }
    }
    private void Start()
    {
        if (_Instance != null) Destroy(this);
        else { _Instance = this; }
        DontDestroyOnLoad(this);
        foreach (TeamController ship in RadarScanner.ships)
        {
            if (!ship.isFriendly) continue;
            Button newShipButton = Instantiate(shipButtonTemplate, transform.position, transform.rotation);
            newShipButton.GetComponentInChildren<TextMeshProUGUI>().text = ship.transform.name;
            newShipButton.onClick.AddListener(ship.transform.GetComponent<UnitController>().ButtonSelect);
            newShipButton.transform.SetParent(transform);
        }
    }
}