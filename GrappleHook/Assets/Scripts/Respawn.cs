using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField]
    float respawnTime;
    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.tag == "Player")
        {
            StartCoroutine(RespawnPlayer(other.transform.root.gameObject.GetComponent<PlayerController>().spawnPoint, other.transform.root.gameObject));
        }
    }

    IEnumerator RespawnPlayer(GameObject spawnPoint, GameObject player)
    {
        yield return new WaitForSeconds(respawnTime);
        player.transform.position = spawnPoint.transform.position;
        player.transform.rotation = spawnPoint.transform.rotation;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        player.GetComponent<NetworkedHook>().hook.transform.position = player.GetComponent<NetworkedHook>().hookStartPosition.transform.position;
        player.GetComponent<NetworkedHook>().rbHook.velocity = Vector3.zero;
        player.GetComponent<NetworkedHook>().hasHookFired = false;
        player.GetComponent<NetworkedHook>().hookReturning = false;
        player.GetComponent<NetworkedHook>().hook.layer = 8;

        if(player.GetComponent<StealBriefCase>().ownBriefcase)
        {
            player.GetComponent<StealBriefCase>().ownBriefcase = false;
            player.GetComponent<StealBriefCase>().briefCase.transform.parent = null;
            player.GetComponent<StealBriefCase>().briefCase.GetComponent<BriefCase>().stealable = true;
            player.GetComponent<StealBriefCase>().briefCase.transform.position = player.GetComponent<StealBriefCase>().briefCase.GetComponent<BriefCase>().startingPosition;
            player.GetComponent<StealBriefCase>().briefCase.transform.rotation = player.GetComponent<StealBriefCase>().briefCase.GetComponent<BriefCase>().startingRotation;
        }
    }
}
