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
    //[SerializeField] private GameObject playerScore;
    //[SerializeField] private Transform scoreBoard;
    //[SerializeField] private GameObject returnText;

    //private Coroutine scoresCo;
    private bool gameOver;
    //private string[] playerNamesOrdered;
    //private float[] playerTimesOrdered;
    //private Color[] playerColoursOrdered;
    //private Color[] playerColours;

    void Start()
    {
        gameOver = false;

        //playerNamesOrdered = new string[PhotonNetwork.PlayerList.Length];
        //playerTimesOrdered = new float[PhotonNetwork.PlayerList.Length];
        //playerColoursOrdered = new Color[PhotonNetwork.PlayerList.Length];

        if (!PhotonNetwork.IsMasterClient)
            return;

        //playerColours = new Color[] { Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.grey, new Color(1f, .25f, 0f, 1f) };

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            int k = 0;
            //if (i < playerPrefabs.Length)
            //    k = i;
            //else
            //{
            //    k = (i  % playerPrefabs.Length);
            //}

            GameObject go = PhotonNetwork.Instantiate(playerPrefabs[i % playerPrefabs.Length].name, Vector3.one, Quaternion.identity);
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
        Invoke("ReturnToLobby", 5 + PhotonNetwork.PlayerList.Length);
        //string[] namesTest = { "pete", "matt", "ben", "gaby" };
        //float[] timesTest = { 60, 49, 25, 22 };
        //photonView.RPC(nameof(SetScores), RpcTarget.AllBuffered, photonView.OwnerActorNr, photonView.Owner.NickName, 1, playerColours[photonView.OwnerActorNr - 1]);
        //DisplayScores();
    }

    //[PunRPC]
    //public void SetScores(int playerID, string playerName, float playerTime, float[] playerColour)
    //{
    //    playerNamesOrdered[playerID] = playerName;
    //    playerTimesOrdered[playerID] = playerTime;
    //    playerColoursOrdered[playerID] = new Vector4(playerColour[0], playerColour[1], playerColour[2], playerColour[3]);
    //}

    //[PunRPC]
    //public void DisplayScores()// string[] names, float[] times)
    //{
    //    if (scoresCo != null) StopCoroutine(scoresCo);
    //    scoresCo = StartCoroutine(ShowPlayerScores());//names, times));
    //}

    //private IEnumerator ShowPlayerScores()//string[] names, float[] times)
    //{
    //    yield return new WaitForSeconds(1f);

    //    for (int i = 1; i < PhotonNetwork.PlayerList.Length; i++)
    //    {
    //        // Spawn a score prefab for each player other than the winner.
    //        GameObject go = Instantiate(playerScore, scoreBoard);
    //        Text[] nameAndTime = go.GetComponentsInChildren<Text>();
    //        // Set player name and time.
    //        nameAndTime[0].text = playerNamesOrdered[i];
    //        nameAndTime[1].text = playerTimesOrdered[i].ToString() + "s";
    //        // Set text colours.
    //        nameAndTime[0].color = playerColoursOrdered[i];
    //        nameAndTime[1].color = playerColoursOrdered[i];
    //        // Wait 1 second between each spawn.
    //        yield return new WaitForSeconds(1f);
    //    }
    //    // Show the "returning to lobby..." text.
    //    returnText.SetActive(true);
    //    // After 5 seconds, return to the lobby.
    //    yield return new WaitForSeconds(5f);
    //    ReturnToLobby();
    //}

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
