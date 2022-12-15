using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GameOverHandler : NetworkBehaviour
{
    public static event Action ServerOnGameOver;
    public static event Action<string> ClientOnGameOver;

    List<UnitBase> unitBases = new List<UnitBase>();

    #region Server
    public override void OnStartServer()
    {
        UnitBase.ServerOnBaseSpawned += UnitBase_ServerOnBaseSpawned;
        UnitBase.ServerOnBaseDespawned += UnitBase_ServerOnBaseDespawned;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnBaseSpawned -= UnitBase_ServerOnBaseSpawned;
        UnitBase.ServerOnBaseDespawned -= UnitBase_ServerOnBaseDespawned;
    }

    [Server]
    void UnitBase_ServerOnBaseSpawned(UnitBase unitBase)
    {
        unitBases.Add(unitBase);
    }

    [Server]
    void UnitBase_ServerOnBaseDespawned(UnitBase unitBase)
    {
        unitBases.Remove(unitBase);

        if (unitBases.Count != 1)
            return;

        int playerId = unitBases[0].connectionToClient.connectionId;

        RpcGameOver($"Player {playerId}");

        ServerOnGameOver?.Invoke();
    }
    #endregion

    #region Client
    [ClientRpc]
    void RpcGameOver(string winner)
    {
        ClientOnGameOver?.Invoke(winner);
    }
    #endregion
}
