///========================
/// Peter Phillips, 2022
/// PlayerInfo.cs
///========================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField] private Text playerName;
    
    private int playerID;

    public void Initialise(string name, int actorID)
    {
        // Set player name text.
        playerName.text = name;
        // Set playerID.
        playerID = actorID;
    }
}
