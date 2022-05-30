using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedHookDetector : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Hookable")
        {
            player.GetComponent<NetworkedHook>().hookedObject = collision.gameObject;
            player.GetComponent<NetworkedHook>().hasHooked = true;
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
}
