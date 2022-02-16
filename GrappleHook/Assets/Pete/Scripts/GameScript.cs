///========================
/// Peter Phillips, 2022
/// GameScript.cs
///========================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameScript : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        //Color[] playerColours = new Color[] { Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.grey, new Color(1f, .25f, 0f, 1f) };

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.one, Quaternion.identity);
            go.GetPhotonView().TransferOwnership(i + 1);
            go.GetPhotonView().RPC("Initialise", RpcTarget.AllBuffered, i);
        }
    }

    // Start method for the test scene.
    //void Start()
    //{
    //    if (!PhotonNetwork.IsMasterClient)
    //        return;
    //
    //    Color[] playerColours = new Color[]{ Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.grey, new Color(1f, .25f, 0f, 1f) };
    //
    //    for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
    //    {
    //        GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.one, Quaternion.identity);
    //        go.GetPhotonView().TransferOwnership(i + 1);
    //        go.GetPhotonView().RPC("Initialise", RpcTarget.AllBuffered, i);
    //    }
    //}
}
