using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Health : NetworkBehaviour
{
    public event Action ServerOnDie;

    public event Action<int, int> ClientOnHealthUpdated;

    [SerializeField] int maxHealth = 100;

    [SyncVar(hook = nameof(HandleHealthUpdated))]
    int currentHealth;

    #region Server
    public override void OnStartServer()
    {
        currentHealth = maxHealth;

        UnitBase.ServerOnPlayerDie += UnitBase_ServerOnPlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= UnitBase_ServerOnPlayerDie;
    }

    [Server]
    public void DealDamage(int damageAmount)
    {
        if (currentHealth == 0)
            return;

        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        if (currentHealth != 0)
            return;

        ServerOnDie?.Invoke();
    }

    [Server]
    void UnitBase_ServerOnPlayerDie(int connectionId)
    {
        if (connectionToClient.connectionId != connectionId)
            return;

        DealDamage(currentHealth);
    }
    #endregion

    #region Client
    void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
    }
    #endregion
}
