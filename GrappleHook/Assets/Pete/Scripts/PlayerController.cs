///========================
/// Peter Phillips, 2022
/// PlayerController.cs
///========================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    [PunRPC]
    private void Initialise(int playerID)
    {
        Color[] playerColours = new Color[] { Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.grey, new Color(1f, .25f, 0f, 1f) };

        transform.position = transform.right * (-7 + 2 * playerID);
        GetComponentInChildren<TextMesh>().text = PhotonNetwork.PlayerList[playerID].NickName;
        GetComponent<SpriteRenderer>().color = playerColours[playerID];
    }
 
    void Update()
    {
        // Break out of update loop if not the owner of this gameobject.
        if (!gameObject.GetPhotonView().AmOwner)
            return;

        if (Input.GetAxis("Horizontal") != 0)
            transform.position += Vector3.right * Input.GetAxis("Horizontal") * Time.deltaTime * 4f;

        if (Input.GetAxis("Vertical") != 0)
            transform.position += Vector3.up * Input.GetAxis("Vertical") * Time.deltaTime * 4f;

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -7f, 7f), Mathf.Clamp(transform.position.y, -3.5f, 3f), 0);
    }
}
