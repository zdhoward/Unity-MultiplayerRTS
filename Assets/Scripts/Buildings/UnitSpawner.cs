using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;
using TMPro;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] Health health = null;
    [SerializeField] Unit unitPrefab = null;
    [SerializeField] Transform unitSpawnPoint = null;
    [SerializeField] TMP_Text remainingUnitsLabel = null;
    [SerializeField] Image unitProgressImage = null;
    [SerializeField] int maxUnitQueue = 5;
    [SerializeField] float spawnMoveRange = 7;
    [SerializeField] float unitSpawnDuration = 5f;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    int queuedUnits;

    [SyncVar]
    float unitTimer;

    float progressImageVelocity;

    void Update()
    {
        if (isServer)
        {
            ProduceUnits();
        }

        if (isClient)
        {
            UpdateTimerDisplay();
        }
    }

    #region Server
    public override void OnStartServer()
    {
        health.ServerOnDie += Health_ServerOnDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= Health_ServerOnDie;
    }

    [Server]
    void ProduceUnits()
    {
        if (queuedUnits == 0)
            return;

        unitTimer += Time.deltaTime;

        if (unitTimer < unitSpawnDuration)
            return;

        GameObject unitInstance = Instantiate(unitPrefab.gameObject, unitSpawnPoint.position, unitSpawnPoint.rotation);

        NetworkServer.Spawn(unitInstance, connectionToClient);

        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = unitSpawnPoint.position.y;

        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

        queuedUnits--;
        unitTimer = 0f;
    }

    [Server]
    void Health_ServerOnDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    void CmdSpawnUnit()
    {
        if (queuedUnits == maxUnitQueue)
            return;

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        if (player.GetResources() < unitPrefab.GetResourceCost())
            return;

        queuedUnits++;

        player.SetResources(player.GetResources() - unitPrefab.GetResourceCost());
    }
    #endregion

    #region Client
    void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuration;

        if (newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(unitProgressImage.fillAmount, newProgress, ref progressImageVelocity, 0.1f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!isOwned)
            return;

        CmdSpawnUnit();
    }

    void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
    {
        remainingUnitsLabel.text = newUnits.ToString();
    }
    #endregion
}
