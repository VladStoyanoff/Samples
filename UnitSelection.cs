using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class UnitSelection : MonoBehaviour
{
    [SerializeField] RectTransform unitSelectionArea;
    [SerializeField] LayerMask layerMask;
    bool isPlacingABuilding;

    BuildingButton buildingButton;
    Vector2 startPosition;
    NetworkPlayerTankio player;

    public List<Unit> SelectedUnits { get; } = new List<Unit>();

    void Start()
    {
        buildingButton = FindObjectOfType<BuildingButton>();
        player = NetworkClient.connection.identity.GetComponent<NetworkPlayerTankio>();
        Unit.AuthorityOnUnitDespawned += Unit_AuthorityOnUnitDespawned;
        GameOverHandler.ClientOnGameOver += GameOverHandler_ClientOnGameOver;
    }

    void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawned -= Unit_AuthorityOnUnitDespawned;
    }

    // If the player is placing a building return. If the left button was pressed this frame, start the selecting area. If the button was released, select the units.
    // And if its currently being presed, update selection area

    void Update()
    {
        if (isPlacingABuilding) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }

        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            SelectUnits();
        }

        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
    }

    // Updates the selection area by using simple maths

    void UpdateSelectionArea()
    {
        var mousePosition = Mouse.current.position.ReadValue();

        var areaWidth = mousePosition.x - startPosition.x;
        var areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    // Starts selection area

    void StartSelectionArea()
    {
        if (!Keyboard.current.leftShiftKey.isPressed)
        {
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Deselect();
            }

            SelectedUnits.Clear();
        }

        unitSelectionArea.gameObject.SetActive(true);

        startPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    // Dynamically selects units. First it tests whether the player is selecting with a mouse click and not a whole area, and then after that selects the units in the whole area
    // if one has already been created

    void SelectUnits()
    {
        unitSelectionArea.gameObject.SetActive(false);

        if(unitSelectionArea.sizeDelta.magnitude == 0)
        {
            var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, layerMask)) return;
            if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) return;
            if (!unit.hasAuthority) return;

            SelectedUnits.Add(unit);

            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Select();
            }
            return;
        }

        var min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        var max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach (Unit unit in player.GetMyUnits())
        {
            if (SelectedUnits.Contains(unit)) continue;

            var screenPosition = Camera.main.WorldToScreenPoint(unit.transform.position);
            
            if(screenPosition.x > min.x &&
               screenPosition.x < max.x &&
               screenPosition.y > min.y &&
               screenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }

    void Unit_AuthorityOnUnitDespawned(Unit unit)
    {
        SelectedUnits.Remove(unit);
    }

    void GameOverHandler_ClientOnGameOver(string winnerName)
    {
        enabled = false;
    }

    public void ActivateBoolIsPlacingABuilding()
    {
        isPlacingABuilding = true;
    }

    public void DisactivateBoolIsPlacingABuilding()
    {
        isPlacingABuilding = false;
    }
}
