using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
// using UnityEngine.InputSystem;
using Mirror;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] NavMeshAgent agent = null;
    [SerializeField] Targeter targeter = null;
    [SerializeField] float chaseRange = 10f;

    // private Camera mainCamera;

    #region Server
    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += GameOverHandler_ServerOnGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= GameOverHandler_ServerOnGameOver;
    }

    [ServerCallback]
    void Update()
    {
        Targetable target = targeter.GetTarget();
        if (target != null)
        {
            if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                agent.SetDestination(target.transform.position);
            }
            else if (agent.hasPath)
            {
                agent.ResetPath();
            }
            return;
        }

        if (!agent.hasPath)
            return;

        if (agent.remainingDistance > agent.stoppingDistance)
            return;

        agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 targetPosition)
    {
        ServerMove(targetPosition);
    }

    [Server]
    public void ServerMove(Vector3 targetPosition)
    {
        targeter.ClearTarget();

        if (!NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            return;

        agent.SetDestination(hit.position);
    }

    [Server]
    void GameOverHandler_ServerOnGameOver()
    {
        agent.ResetPath();
    }
    #endregion

    // #region Client
    // public override void OnStartAuthority()
    // {
    //     mainCamera = Camera.main;
    // }

    // [ClientCallback]
    // private void Update()
    // {
    //     if (!isOwned)
    //         return;

    //     if (!Mouse.current.rightButton.wasPressedThisFrame)
    //         return;

    //     Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
    //     if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
    //         return;

    //     CmdMove(hit.point);
    // }
    // #endregion
}
