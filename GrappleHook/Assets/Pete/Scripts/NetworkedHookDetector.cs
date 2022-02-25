using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedHookDetector : MonoBehaviour
{
    [SerializeField]
    GameObject player;   

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Hookable")
        {
            player.GetComponent<NetworkedHook>().hookedObject = other.gameObject;
            player.GetComponent<NetworkedHook>().hasHooked = true;
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
}
