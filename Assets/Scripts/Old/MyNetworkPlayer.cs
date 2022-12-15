using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class MyNetworkPlayer : NetworkBehaviour
{
    [SerializeField] private TMP_Text displayNameText = null;
    [SerializeField] private Renderer displayColorRenderer = null;

    [SyncVar(hook = nameof(HandleDisplayNameUpdated))][SerializeField] string displayName = "Missing Name";
    [SyncVar(hook = nameof(HandleDisplayColorUpdated))][SerializeField] Color displayColor = Color.black;

    #region Server
    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [Server]
    public void SetDisplayColor(Color displayColor)
    {
        this.displayColor = displayColor;
    }

    [Command]
    private void CmdSetDisplayName(string newDisplayName)
    {
        if (!ValidateName(newDisplayName))
        {
            Debug.Log("Name is invalid");
            return;
        }

        RpcLogNewName(newDisplayName);
        SetDisplayName(newDisplayName);
    }
    #endregion

    #region Client
    private void HandleDisplayNameUpdated(string oldName, string newName)
    {
        displayNameText.text = newName;
    }

    private void HandleDisplayColorUpdated(Color oldColor, Color newColor)
    {
        displayColorRenderer.material.SetColor("_BaseColor", newColor);
    }

    [ContextMenu("SetMyName")]
    private void SetMyName()
    {
        CmdSetDisplayName("My New Name is shit");
    }

    [ClientRpc]
    private void RpcLogNewName(string newName)
    {
        Debug.Log(newName);
    }
    #endregion

    #region Validations
    private bool ValidateName(string nameToValidate)
    {
        string whitelistedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ ";
        string[] blacklistedWords = { "fuck", "shit" };

        foreach (char character in nameToValidate)
        {
            if (!whitelistedCharacters.Contains(character))
                return false;
        }

        foreach (string word in blacklistedWords)
        {
            if (nameToValidate.Contains(word.ToLower()))
                return false;
        }

        return true;
    }
    #endregion
}
