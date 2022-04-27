///========================
/// Peter Phillips, 2022
/// Lobbyscript.cs
///========================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyScript : MonoBehaviourPunCallbacks
{
    [Header("Login Screen")]
    [SerializeField] private GameObject loginScreen;
    [SerializeField] private InputField playerNameInput;
    [SerializeField] private GameObject playerNameWarning;

    [Header("Information Panel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Text infoText;

    [Header("Lobby Browser")]
    [SerializeField] private GameObject lobbyBrowser;
    [SerializeField] private GameObject joinGameButton;
    [SerializeField] private GameObject lobbyInfo;
    [SerializeField] private Transform LobbyList;

    [Header("Host Game Screen")]
    [SerializeField] private GameObject hostGameScreen;
    [SerializeField] private InputField lobbyNameInput;
    [SerializeField] private GameObject lobbyNameWarning;

    [Header("Lobby Screen")]
    [SerializeField] private GameObject lobbyScreen;
    [SerializeField] private Text lobbyName;
    [SerializeField] private Text playerCount;
    [SerializeField] private GameObject playerInfo;
    [SerializeField] private Transform playerList;


    private GameObject[] UIPanels;
    private byte maxPlayers = 8;
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    private Dictionary<string, GameObject> roomListEntries = new Dictionary<string, GameObject>();
    private Dictionary<int, GameObject> playerListEntries;
    private string roomToJoin;

    #region MonoBehaviour

    private void Awake()
    {
        // Set autosync = true to expedite loading into the game after joining a lobby.
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
        Cursor.lockState = CursorLockMode.None;

        UIPanels = new GameObject[]
        {
            loginScreen, 
            infoPanel, 
            lobbyBrowser, 
            hostGameScreen,
            lobbyScreen
        };

        UpdateRoomListView();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (loginScreen.activeInHierarchy)
                OnLoginButtonClicked();
            else if (hostGameScreen.activeInHierarchy)
                OnCreateLobbyPressed();
        }
    }

    #endregion //MonoBehaviour

    #region PUN Callbacks

    public override void OnConnectedToMaster()
    {
        // Once the client connects to the server, switch the UI panel to the lobby browser.
        SetUIPanel(lobbyBrowser);
        // Join the lobby to allow player to find a game room.
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("JOINED ROOM");
        cachedRoomList.Clear();

        // Leave lobby if currently in one.
        if (PhotonNetwork.InLobby)
        {
            Debug.Log("LEFT LOBBY");
            PhotonNetwork.LeaveLobby();
        }

        // Once the client joins a room, switch the UI panel to the lobby screen.
        SetUIPanel(lobbyScreen);
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
            GameObject info = Instantiate(playerInfo);
            info.transform.SetParent(playerList);
            info.transform.localScale = Vector3.one;
            info.GetComponent<PlayerInfo>().Initialise(p.NickName, p.ActorNumber);

            playerListEntries.Add(p.ActorNumber, info);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Clear out and repopulate the list of available lobbies.
        ClearRoomListView();
        UpdateCachedRoomList(roomList);
        UpdateRoomListView();
    }

    public override void OnJoinedLobby()
    {
        // When a player joins the lobby, clear any previous room lists.
        cachedRoomList.Clear();
        ClearRoomListView();
    }

    public override void OnLeftLobby()
    {
        cachedRoomList.Clear();
        ClearRoomListView();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Create player info prefab.
        GameObject entry = Instantiate(playerInfo);
        entry.transform.SetParent(playerList);
        entry.transform.localScale = Vector3.one;
        entry.GetComponent<PlayerInfo>().Initialise(newPlayer.NickName, newPlayer.ActorNumber);
        // Add player to dictionary.
        playerListEntries.Add(newPlayer.ActorNumber, entry);
        // Update player count.
        playerCount.text = PhotonNetwork.CurrentRoom.PlayerCount + " / " + maxPlayers;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (playerListEntries.ContainsKey(otherPlayer.ActorNumber))
        {
            Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
            playerListEntries.Remove(otherPlayer.ActorNumber);
        }
        Debug.Log(playerListEntries.Count);
    }

    #endregion //PUN Callbacks

    #region UI Callbacks

    public void OnBackButtonLobbyBrowserPressed()
    {
        // Switch the UI panel.
        SetUIPanel(loginScreen);
        // Deactivate the Join Game button.
        joinGameButton.SetActive(false);
        // Disconnect from the matchmaking server.
        PhotonNetwork.Disconnect();
    }

    public void OnBackButtonHostGamePressed()
    {
        // Switch the UI panel.
        SetUIPanel(lobbyBrowser);
        // Reset the lobby name text field.
        lobbyNameInput.text = "";
        // Remove name warning (if it's active).
        lobbyNameWarning.SetActive(false);
    }

    public void OnBackButtonLobbyPressed()
    {
        // Switch the UI panel.
        SetUIPanel(lobbyBrowser);
        // Leave the room.
        PhotonNetwork.LeaveRoom();
        // Clear the player list.
        foreach (KeyValuePair<int, GameObject> entry in playerListEntries)
            Destroy(playerListEntries[entry.Key].gameObject);
        playerListEntries.Clear();
        Debug.Log("CLEARED PLAYER LIST");
    }

    public void OnLoginButtonClicked()
    {
        Debug.Log("LOGIN");
        string playerName = playerNameInput.text;
        // Check player has entered a name.
        if (playerName != "")
        {
            // If so, set the info text and switch the UI panel to the information panel.
            infoText.text = "Connecting...";
            SetUIPanel(infoPanel);
            // Connect to the network and set the client's nickname to the input name.
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
            // Remove name warning (if it's active).
            playerNameWarning.SetActive(false);
        }
        // If no name is entered, display a warning message.
        else
        {
            Debug.LogWarning("Player Name is invalid.");
            playerNameWarning.SetActive(true);
        }
    }

    public void OnLobbyPressed(string lobbyName)
    {
        // Activate the Join Game button.
        joinGameButton.SetActive(true);
        // Set the lobby to join.
        roomToJoin = lobbyName;
        Debug.Log(roomToJoin);
    }

    public void OnHostGamePressed()
    {
        // Deactivate the Join Game button.
        joinGameButton.SetActive(false);
        // Switch UI panel to the host game panel.
        SetUIPanel(hostGameScreen);
    }

    public void OnCreateLobbyPressed()
    {
        Debug.Log("CREATE");
        string lobbyName = lobbyNameInput.text;
        // Check player has entered a name.
        if (lobbyName != "")
        {
            // If so, set the info text and switch the UI panel to the information panel.
            infoText.text = "Creating lobby...";
            SetUIPanel(infoPanel);
            // Create lobby.
            RoomOptions options = new RoomOptions {MaxPlayers = maxPlayers, PlayerTtl = 1000 };
            PhotonNetwork.CreateRoom(lobbyName, options, null);
            // Remove name warning (if it's active).
            lobbyNameWarning.SetActive(false);
            // Reset the lobby name text field.
            lobbyNameInput.text = "";
        }
        // If no name is entered, display a warning message.
        else
        {
            Debug.LogWarning("Lobby Name is invalid.");
            lobbyNameWarning.SetActive(true);
        }
    }

    public void OnJoinGamePressed()
    {
        // Deactivate the Join Game button.
        joinGameButton.SetActive(false);
        // Set the info text and switch the UI panel to the information panel.
        infoText.text = "Joining lobby...";
        SetUIPanel(infoPanel);

        PhotonNetwork.JoinRoom(roomToJoin);
        //PhotonNetwork.JoinRandomRoom();
    }

    public void OnStartGamePressed()
    {
        // Load the game scene via photon so that everyone loads in at once.
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("WhiteBox");
    }

    #endregion //UI Callbacks

    #region Functions

    private void SetUIPanel(GameObject activePanel)
    {
        // Deactivate all panels.
        foreach (GameObject go in UIPanels)
            go.SetActive(false);
        // Activate specified panel.
        activePanel.SetActive(true);
    }

    private void ClearRoomListView()
    {
        // Remove all instances of lobby info.
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }
        // Empty the room list dictionary.
        roomListEntries.Clear();
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed.
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }
                
                continue;
            }

            // Update cached room info.
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            // Add new room info to cache.
            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    private void UpdateRoomListView()
    {
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            Debug.Log("adding room to list");

            GameObject entry = Instantiate(lobbyInfo);
            entry.transform.SetParent(LobbyList);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<LobbyInfo>().Initialise(info.Name, (byte)info.PlayerCount, info.MaxPlayers);

            roomListEntries.Add(info.Name, entry);
        }
    }

    #endregion //Functions
}