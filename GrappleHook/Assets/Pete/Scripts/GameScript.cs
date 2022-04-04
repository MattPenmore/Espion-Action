///========================
/// Peter Phillips, 2022
/// GameScript.cs
///========================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameScript : MonoBehaviourPun
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Canvas winScreen;
    [SerializeField] private Text winnerText;

    private bool gameOver;

    void Start()
    {
        gameOver = false;

        if (!PhotonNetwork.IsMasterClient)
            return;

        //Color[] playerColours = new Color[] { Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.grey, new Color(1f, .25f, 0f, 1f) };

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.one, Quaternion.identity);
            go.GetPhotonView().TransferOwnership(i + 1);
            go.GetComponent<PlayerController>().hookObject.GetPhotonView().TransferOwnership(i + 1);
            go.GetPhotonView().RPC("Initialise", RpcTarget.AllBuffered, i);
        }
    }

    [PunRPC]
    public void EndGame(string playerName, float[] playerColour)
    {
        if (gameOver) return;

        gameOver = true;
        winnerText.text = playerName + " Wins!";
        Vector4 pColour = new Vector4(playerColour[0], playerColour[1], playerColour[2], playerColour[3]);
        winnerText.color = pColour;
        winScreen.enabled = true;

        Invoke("ReturnToLobby", 5);
    }

    private void ReturnToLobby()
    {
        //DontDestroyOnLoad(this);
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("Lobby");
        //PhotonNetwork.LoadLevel("Lobby");
        //Destroy(this);
    }    
}
