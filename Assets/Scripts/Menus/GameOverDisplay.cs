using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] GameObject gameOverDisplayParent = null;
    [SerializeField] TMP_Text winnerNameText = null;

    void Start()
    {
        GameOverHandler.ClientOnGameOver += GameOverHandler_ClientOnGameOver;
    }

    void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= GameOverHandler_ClientOnGameOver;
    }

    public void LeaveGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            // stop hosting
            NetworkManager.singleton.StopHost();
        }
        else
        {
            // stop client
            NetworkManager.singleton.StopClient();
        }
    }

    void GameOverHandler_ClientOnGameOver(string winner)
    {
        winnerNameText.text = $"{winner} has Won!";
        gameOverDisplayParent.SetActive(true);
    }

}
