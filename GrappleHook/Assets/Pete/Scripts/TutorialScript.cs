using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class TutorialScript : MonoBehaviourPunCallbacks
{
    public GameObject startGameButton;
    public Canvas interactCanvas;

    [SerializeField] private GameObject playerPrefab;

    [Header("Lobby Screen")]
    [SerializeField] private Text lobbyName;
    [SerializeField] private Text playerCount;
    [SerializeField] private GameObject playerInfo;
    [SerializeField] private Transform playerList;
    [SerializeField] private Text gameStartingText;
    [SerializeField] private Canvas gameStartingCanvas;

    private GameObject[] players;
    private byte maxPlayers = 8;
    private Dictionary<int, GameObject> playerListEntries;

    private Coroutine startGameCoroutine;

    void Start()
    {
        gameStartingCanvas.enabled = false;

        // Leave lobby if currently in one.
        if (PhotonNetwork.InLobby)
        {
            Debug.Log("LEFT LOBBY");
            PhotonNetwork.LeaveLobby();
        }

        // Display lobby name and player count.
        lobbyName.text = PhotonNetwork.CurrentRoom.Name;
        playerCount.text = PhotonNetwork.CurrentRoom.PlayerCount + " / " + maxPlayers;
        // Create dictionary of players.
        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }
        // Instantiate player info prefabs in a list and populate the dictionary.
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            GameObject info = Instantiate(playerInfo, playerList);
            info.transform.localScale = Vector3.one;
            info.GetComponent<PlayerInfo>().Initialise(p.NickName, p.ActorNumber);

            playerListEntries.Add(p.ActorNumber, info);
        }

        if (!PhotonNetwork.IsMasterClient)
            return;

        players = new GameObject[PhotonNetwork.PlayerList.Length];
        Color[] playerColours = new Color[] { Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.grey, new Color(1f, .25f, 0f, 1f) };

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.one, Quaternion.identity);
            go.GetPhotonView().TransferOwnership(i + 1);
            go.GetComponent<PlayerController>().hookObject.GetPhotonView().TransferOwnership(i + 1);
            go.GetPhotonView().RPC("Initialise", RpcTarget.AllBuffered, i, true);
            players[i] = go;
        }
    }   

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (Input.GetKeyDown(KeyCode.G))
        {
            //foreach (GameObject go in players)
            //    PhotonNetwork.Destroy(go);
            //PhotonNetwork.LoadLevel("Lobby");
            //PhotonNetwork.LoadLevel("WhiteBox");
            StartGame();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Create player info prefab.
        GameObject entry = Instantiate(playerInfo, playerList);
        entry.transform.localScale = Vector3.one;
        entry.GetComponent<PlayerInfo>().Initialise(newPlayer.NickName, newPlayer.ActorNumber);
        // Add player to dictionary.wwwwwwwwwwwwwwwwwwwwwwwwwwwwww
        playerListEntries.Add(newPlayer.ActorNumber, entry);
        // Update player count.
        playerCount.text = PhotonNetwork.CurrentRoom.PlayerCount + " / " + maxPlayers;

        if (!PhotonNetwork.IsMasterClient)
            return;

        GameObject[] temp = players;
        players = new GameObject[PhotonNetwork.PlayerList.Length];
        for (int i = 0; i < temp.Length; i++)
            players[i] = temp[i];
        GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.one, Quaternion.identity);
        go.GetPhotonView().TransferOwnership(PhotonNetwork.PlayerList.Length);
        go.GetComponent<PlayerController>().hookObject.GetPhotonView().TransferOwnership(PhotonNetwork.PlayerList.Length);
        go.GetPhotonView().RPC("Initialise", RpcTarget.AllBuffered, PhotonNetwork.PlayerList.Length - 1, true);
        players[players.Length - 1] = go;
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        if (player.ActorNumber == 1) //master
            PhotonNetwork.LoadLevel("Lobby");

        // Delete player info prefab and remove from 
        if (playerListEntries.ContainsKey(player.ActorNumber))
        {
            Destroy(playerListEntries[player.ActorNumber].gameObject);
            playerListEntries.Remove(player.ActorNumber);
        }
        // Update player count.
        playerCount.text = PhotonNetwork.CurrentRoom.PlayerCount + " / " + maxPlayers;
    }

    [PunRPC]
    public void StartGame()
    {
        if (startGameCoroutine != null) StopCoroutine(startGameCoroutine);
        startGameCoroutine = StartCoroutine(StartGameCountdown());
    }

    private IEnumerator StartGameCountdown()
    {
        // Set countdown text, activate canvas, and change button colour.
        gameStartingText.text = "Starting in 3...";
        gameStartingCanvas.enabled = true;
        startGameButton.GetComponent<Renderer>().material.color = Color.green;
        startGameButton.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.green);
        yield return new WaitForSeconds(1);
        gameStartingText.text = "Starting in 2...";
        yield return new WaitForSeconds(1);
        gameStartingText.text = "Starting in 1...";
        yield return new WaitForSeconds(1);
        // Master client starts the game.
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("Game");
    }
}
