using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TutorialScript : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;

    private GameObject[] players;

    void Start()
    {
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
            PhotonNetwork.LoadLevel("WhiteBox");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
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
    }
}
