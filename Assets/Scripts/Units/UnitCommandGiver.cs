using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : MonoBehaviour
{
    [SerializeField] UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] LayerMask layerMask = new LayerMask();

    Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        GameOverHandler.ClientOnGameOver += GameOverHandler_ClientOnGameOver;
    }

    void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= GameOverHandler_ClientOnGameOver;
    }

    void Update()
    {
        if (!Mouse.current.rightButton.wasPressedThisFrame)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            return;

        if (hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {
            if (target.isOwned)
            {
                TryMove(hit.point);
                return;
            }

            TryTarget(target);
            return;
        }

        TryMove(hit.point);
    }

    void TryMove(Vector3 point)
    {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
            unit.GetUnitMovement().CmdMove(point);
    }

    void TryTarget(Targetable target)
    {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
            unit.GetTargeter().CmdSetTarget(target.gameObject);
    }

    void GameOverHandler_ClientOnGameOver(string winnerName)
    {
        enabled = false;
    }
}
