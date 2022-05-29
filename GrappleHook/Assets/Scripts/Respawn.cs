using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Respawn : MonoBehaviour
{
    [SerializeField]
    float respawnTime;

    [SerializeField]
    private Image whiteScreen;

    private float fadeInProportion = 8;

    private void Start()
    {
        //whiteScreen = GameObject.FindGameObjectWithTag("WhiteScreen").GetComponent<Image>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.tag == "Player")
        {
            if(!other.transform.root.gameObject.GetComponent<PlayerController>().respawning && other.transform.root.gameObject.GetPhotonView().IsMine)
                StartCoroutine(RespawnPlayer(other.transform.root.gameObject.GetComponent<PlayerController>().spawnPoint, other.transform.root.gameObject));
        }
    }

    public void RespawnPlayerPush(GameObject player)
    {
        StartCoroutine(RespawnPlayer(player.GetComponent<PlayerController>().spawnPoint, player));
    }

    IEnumerator RespawnPlayer(GameObject spawnPoint, GameObject player)
    {
        Debug.Log("RESPAWNING");

        float localTimer = 0f;

        player.GetComponent<PlayerController>().respawning = true;
        if (StealBriefCase.ownBriefcase)
        {
            StealBriefCase.ownBriefcase = false;
            player.GetComponent<StealBriefCase>().briefCase.transform.parent = null;
            player.GetComponent<StealBriefCase>().briefCase.GetComponent<BriefCase>().stealable = true;
            player.GetComponent<StealBriefCase>().briefCase.transform.position = player.GetComponent<StealBriefCase>().briefCase.GetComponent<BriefCase>().startingPosition;
            player.GetComponent<StealBriefCase>().briefCase.transform.rotation = player.GetComponent<StealBriefCase>().briefCase.GetComponent<BriefCase>().startingRotation;
            StealBriefCase.ownedTime += 5;
            player.GetComponent<StealBriefCase>().CallBriefcaseTransfer(0, false, StealBriefCase.ownedTime);
        }

        // Turn on the white screen.
        whiteScreen.GetComponentInParent<Canvas>().enabled = true;
        Color tempCol = Color.white;

        while (localTimer < respawnTime / fadeInProportion)
        {
            // Fade in the white screen.
            tempCol.a = localTimer / respawnTime * fadeInProportion;
            whiteScreen.color = tempCol;

            localTimer += Time.deltaTime;
            yield return null;
        }
        // Set alpha to 100%.
        tempCol.a = 1;
        whiteScreen.color = tempCol;
        // Do nothing while we wait to respawn.
        while (localTimer < respawnTime)
        {
            localTimer += Time.deltaTime;
            yield return null;
        }
        //yield return new WaitForSeconds(respawnTime);
        player.transform.position = spawnPoint.transform.position;
        player.transform.rotation = spawnPoint.transform.rotation;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        player.GetComponent<NetworkedHook>().hook.transform.position = player.GetComponent<NetworkedHook>().hookStartPosition.transform.position;
        player.GetComponent<NetworkedHook>().rbHook.velocity = Vector3.zero;
        player.GetComponent<NetworkedHook>().hasHookFired = false;
        player.GetComponent<NetworkedHook>().hookReturning = false;
        player.GetComponent<NetworkedHook>().hook.layer = 8;
        player.GetComponent<PlayerController>().respawning = false;
        player.GetComponent<PlayerController>().playedRespawnSound = false;

        // Turn off the white screen.
        whiteScreen.GetComponentInParent<Canvas>().enabled = false;
    }
}
