using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class Unit : NetworkBehaviour
{
    [SerializeField] int resourceCost = 10;
    [SerializeField] Health health = null;
    [SerializeField] UnitMovement unitMovement = null;
    [SerializeField] Targeter targeter = null;
    [SerializeField] UnityEvent onSelected = null;
    [SerializeField] UnityEvent onDeselected = null;

    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;

    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    public UnitMovement GetUnitMovement() => unitMovement;
    public Targeter GetTargeter() => targeter;
    public int GetResourceCost() => resourceCost;

    #region Server
    public override void OnStartServer()
    {
        ServerOnUnitSpawned?.Invoke(this);
        health.ServerOnDie += Health_ServerOnDie;
    }

    public override void OnStopServer()
    {
        ServerOnUnitDespawned?.Invoke(this);
        health.ServerOnDie -= Health_ServerOnDie;
    }

    void Health_ServerOnDie()
    {
        NetworkServer.Destroy(gameObject);
    }
    #endregion

    #region Client
    public override void OnStartAuthority()
    {
        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!isOwned)
            return;

        AuthorityOnUnitDespawned?.Invoke(this);
    }

    [Client]
    public void Select()
    {
        if (!isOwned)
            return;

        onSelected?.Invoke();
    }

    [Client]
    public void Deselect()
    {
        if (!isOwned)
            return;

        onDeselected?.Invoke();
    }
    #endregion
}
