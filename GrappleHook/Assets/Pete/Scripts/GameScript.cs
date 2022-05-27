///========================
/// Peter Phillips, 2022
/// GameScript.cs
///========================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameScript : MonoBehaviourPunCallbacks
{
    public Canvas interactCanvas;
   
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Canvas winScreen;
    [SerializeField] private Text winnerText;

    public bool gameEnded;

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
            go.GetPhotonView().RPC("Initialise", RpcTarget.AllBuffered, i, false);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Debug.Log("SPECTATOR JOINED");  
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        if (player.ActorNumber == 1) //master
            PhotonNetwork.LoadLevel("Lobby");
    }

    [PunRPC]
    public void EndGame(string playerName, float[] playerColour)
    {
        if (gameOver) return;

        gameOver = true;
        winnerText.text = playerName + " Wins!";
        Vector4 pColour = new Vector4(playerColour[0], playerColour[1], playerColour[2], playerColour[3]);
        winnerText.color = pColour;
        DisablePlayerUI();
        winScreen.enabled = true;
        gameEnded = true;
        Invoke("ReturnToLobby", 5);
    }

    private void ReturnToLobby()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("Lobby");
    }

    public void DisablePlayerUI()
    {
        // Enable the player UI.
        GameObject playerUI = GameObject.FindGameObjectWithTag("PlayerUI");

        if (playerUI != null)
        {
            Canvas[] canvasses = playerUI.GetComponentsInChildren<Canvas>();
            foreach (Canvas c in canvasses)
                c.enabled = false;
        }
    }
}
