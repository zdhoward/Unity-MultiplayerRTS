using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] Building[] buildings = new Building[0];

    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    int resources = 500;

    public event Action<int> ClientOnResourcesUpdated;

    List<Unit> myUnits = new List<Unit>();
    List<Building> myBuildings = new List<Building>();

    public int GetResources() => resources;
    public List<Unit> GetMyUnits() => myUnits;
    public List<Building> GetMyBuildings() => myBuildings;

    [Server]
    public void SetResources(int newResources)
    {
        resources = newResources;
    }

    #region Server
    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += Unit_ServerOnUnitSpawned;
        Unit.ServerOnUnitDespawned += Unit_ServerOnUnitDespawned;

        Building.ServerOnBuildingSpawned += Building_ServerOnBuildingSpawned;
        Building.ServerOnBuildingDespawned += Building_ServerOnBuildingDespawned;
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= Unit_ServerOnUnitSpawned;
        Unit.ServerOnUnitDespawned -= Unit_ServerOnUnitDespawned;

        Building.ServerOnBuildingSpawned -= Building_ServerOnBuildingSpawned;
        Building.ServerOnBuildingDespawned -= Building_ServerOnBuildingDespawned;
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 point)
    {
        Building buildingToPlace = null;
        foreach (Building building in buildings)
        {
            if (building.GetId() == buildingId)
            {
                buildingToPlace = building;
                break;
            }
        }

        if (buildingToPlace == null)
            return;

        GameObject buildingInstance = Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);

        NetworkServer.Spawn(buildingInstance, connectionToClient);
    }

    void Unit_ServerOnUnitSpawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        myUnits.Add(unit);
    }

    void Unit_ServerOnUnitDespawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        myUnits.Remove(unit);
    }

    void Building_ServerOnBuildingSpawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        myBuildings.Add(building);
    }

    void Building_ServerOnBuildingDespawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        myBuildings.Remove(building);
    }
    #endregion

    #region Client
    public override void OnStartAuthority()
    {
        if (NetworkServer.active)
            return;

        Unit.AuthorityOnUnitSpawned += Unit_AuthorityOnUnitSpawned;
        Unit.AuthorityOnUnitDespawned += Unit_AuthorityOnUnitDespawned;

        Building.AuthorityOnBuildingSpawned += Building_AuthorityOnBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += Building_AuthorityOnBuildingDespawned;
    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !isOwned)
            return;

        Unit.AuthorityOnUnitSpawned -= Unit_AuthorityOnUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= Unit_AuthorityOnUnitDespawned;

        Building.AuthorityOnBuildingSpawned -= Building_AuthorityOnBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= Building_AuthorityOnBuildingDespawned;
    }

    void ClientHandleResourcesUpdated(int oldResources, int newResources)
    {
        ClientOnResourcesUpdated?.Invoke(newResources);
    }

    void Unit_AuthorityOnUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }

    void Unit_AuthorityOnUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    void Building_AuthorityOnBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
    }

    void Building_AuthorityOnBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }
    #endregion
}
