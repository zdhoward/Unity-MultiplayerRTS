using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Mirror;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] LayerMask unitLayerMask = new LayerMask();
    [SerializeField] RectTransform unitSelectionArea = null;

    RTSPlayer player;
    Camera mainCamera;

    Vector2 startDragPosition;

    public List<Unit> SelectedUnits { get; } = new List<Unit>();

    void Start()
    {
        mainCamera = Camera.main;
        //player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        Unit.AuthorityOnUnitDespawned += Unit_AuthorityOnUnitDespawned;

        GameOverHandler.ClientOnGameOver += GameOverHandler_ClientOnGameOver;
    }

    void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawned -= Unit_AuthorityOnUnitDespawned;

        GameOverHandler.ClientOnGameOver -= GameOverHandler_ClientOnGameOver;
    }

    void Update()
    {
        if (player == null)
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
    }

    void StartSelectionArea()
    {
        if (!Keyboard.current.leftShiftKey.isPressed)
        {
            // Start selection area
            foreach (Unit selectedUnit in SelectedUnits)
                selectedUnit.Deselect();

            SelectedUnits.Clear();
        }

        unitSelectionArea.gameObject.SetActive(true);

        startDragPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startDragPosition.x;
        float areaHeight = mousePosition.y - startDragPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startDragPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);

        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, unitLayerMask))
                return;

            if (!hit.collider.TryGetComponent<Unit>(out Unit unit))
                return;

            if (!unit.isOwned)
                return;

            SelectedUnits.Add(unit);

            foreach (Unit selectedUnit in SelectedUnits)
                selectedUnit.Select();

            return;
        }

        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach (Unit unit in player.GetMyUnits())
        {
            if (SelectedUnits.Contains(unit))
                continue;

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
            if (screenPosition.x > min.x && screenPosition.x < max.x && screenPosition.y > min.y && screenPosition.y < max.y)
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
}
