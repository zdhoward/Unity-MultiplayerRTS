using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text resourcesLabel = null;

    RTSPlayer player;

    void Update()
    {
        if (player == null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

            if (player != null)
            {
                Player_ClientOnResourcesUpdated(player.GetResources());
                player.ClientOnResourcesUpdated += Player_ClientOnResourcesUpdated;
            }
        }
    }

    void OnDestroy()
    {
        player.ClientOnResourcesUpdated -= Player_ClientOnResourcesUpdated;
    }

    void Player_ClientOnResourcesUpdated(int resources)
    {
        resourcesLabel.text = $"Resources: {resources}";
    }
}
