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
    public bool gameEnded;
   
    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Canvas winScreen;
    [SerializeField] private Text winnerText;
    [SerializeField] private GameObject playerScore;
    [SerializeField] private Transform scoreBoard;
    [SerializeField] private GameObject returnText;

    private bool gameOver;
    private GameObject[] playerModels;
    private Color[] playerColours;

    void Start()
    {
        gameOver = false;

        playerColours = new Color[] { Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.grey, new Color(1f, .25f, 0f, 1f) };
        
        if (!PhotonNetwork.IsMasterClient)
            return;

        playerModels = new GameObject[PhotonNetwork.PlayerList.Length];

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            GameObject go = PhotonNetwork.Instantiate(playerPrefabs[i % playerPrefabs.Length].name, Vector3.one, Quaternion.identity);
            go.GetPhotonView().TransferOwnership(i + 1);
            go.GetComponent<PlayerController>().hookObject.GetPhotonView().TransferOwnership(i + 1);
            go.GetPhotonView().RPC("Initialise", RpcTarget.AllBuffered, i, false);
            playerModels[i] = go;
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
    public void EndGame(string playerName, int actorID)
    {
        if (gameOver) return;

        gameOver = true;
        winnerText.text = playerName + " Wins!";
        winnerText.color = playerColours[actorID - 1];
        DisablePlayerUI();
        winScreen.enabled = true;
        gameEnded = true;
        Invoke("ReturnToLobby", 5 + PhotonNetwork.PlayerList.Length);
    }

    private void ReturnToLobby()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("Lobby");
    }

    public void DisablePlayerUI()
    {
        // Disable the player UI.
        GameObject playerUI = GameObject.FindGameObjectWithTag("PlayerUI");

        if (playerUI != null)
        {
            Canvas[] canvasses = playerUI.GetComponentsInChildren<Canvas>();
            foreach (Canvas c in canvasses)
                c.enabled = false;
        }

        interactCanvas.gameObject.SetActive(false);
    }
}
