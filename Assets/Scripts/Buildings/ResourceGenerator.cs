using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ResourceGenerator : NetworkBehaviour
{
    [SerializeField] Health health = null;
    [SerializeField] int resourcesPerInterval = 10;
    [SerializeField] float interval = 2f;

    float timer;
    RTSPlayer player;

    public override void OnStartServer()
    {
        timer = interval;
        player = connectionToClient.identity.GetComponent<RTSPlayer>();

        health.ServerOnDie += Health_ServerOnDie;
        GameOverHandler.ServerOnGameOver += GameOverHandler_ServerOnGameOver;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= Health_ServerOnDie;
        GameOverHandler.ServerOnGameOver -= GameOverHandler_ServerOnGameOver;
    }

    [ServerCallback]
    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            player.SetResources(player.GetResources() + resourcesPerInterval);

            timer += interval;
        }
    }

    void Health_ServerOnDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    void GameOverHandler_ServerOnGameOver()
    {
        enabled = false;
    }
}
