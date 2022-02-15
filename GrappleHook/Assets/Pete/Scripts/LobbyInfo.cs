///========================
/// Peter Phillips, 2022
/// LobbyInfo.cs
///========================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class LobbyInfo : MonoBehaviour
{
    [SerializeField] private Text lobbyName;
    [SerializeField] private Text playerCount;
    
    [SerializeField] private Button selectLobbyButton;

    private LobbyScript lobbyScript;

    public void Start()
    {
        //lobbyScript = GameObject.Find("UI").GetComponent<LobbyScript>();
        //if (lobbyScript == null) return;
        //
        //// Give the button an OnClick event.
        //selectLobbyButton.onClick.AddListener(() =>
        //{
        //    Debug.Log("lobby button pressed.");
        //    // Leave lobby if currently in one.
        //    if (PhotonNetwork.InRoom)
        //    {
        //        PhotonNetwork.LeaveRoom();
        //    }
        //
        //    SetLobbyName();
        //
        //    //lobbyScript.OnLobbyPressed(GetLobbyName());
        //});
    }

    public void Initialise(string name, byte currentPlayers, byte maxPlayers)
    {
        // Set lobby name text.
        lobbyName.text = name;
        // Set player count text.
        playerCount.text = currentPlayers + " / " + maxPlayers;

        lobbyScript = GameObject.Find("UI").GetComponent<LobbyScript>();
        if (lobbyScript == null) return;

        // Give the button an OnClick event.
        selectLobbyButton.onClick.AddListener(() =>
        {
            Debug.Log("lobby button pressed.");
            // Leave room if currently in one.
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }

            //SetLobbyName();

            lobbyScript.OnLobbyPressed(name);
        });
    }

    //public string GetLobbyName()
    //{
    //    return lobbyName.name;
    //}
    //
    //public void SetLobbyName()
    //{
    //    string lobbyName = GetLobbyName();
    //    lobbyScript.SetLobbyName(lobbyName);
    //}
}
