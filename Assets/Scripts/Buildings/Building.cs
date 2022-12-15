using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Building : NetworkBehaviour
{
    public static event Action<Building> ServerOnBuildingSpawned;
    public static event Action<Building> ServerOnBuildingDespawned;

    public static event Action<Building> AuthorityOnBuildingSpawned;
    public static event Action<Building> AuthorityOnBuildingDespawned;

    [SerializeField] GameObject buildingPreview;
    [SerializeField] Sprite icon = null;
    [SerializeField] int id = -1;
    [SerializeField] int price = 100;

    public GameObject GetBuildingPreview() => buildingPreview;
    public Sprite GetIcon() => icon;
    public int GetId() => id;
    public int GetPrice() => price;

    #region Server
    public override void OnStartServer()
    {
        ServerOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBuildingDespawned?.Invoke(this);
    }
    #endregion

    #region Client
    public override void OnStartAuthority()
    {
        AuthorityOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!isOwned)
            return;

        AuthorityOnBuildingDespawned?.Invoke(this);
    }
    #endregion
}
