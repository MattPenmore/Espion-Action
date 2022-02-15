using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookDetector : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if(collision.gameObject.tag == "Hookable")
    //    {
    //        player.GetComponent<GrapplingHook>().hookedObject = collision.gameObject;
    //        player.GetComponent<GrapplingHook>().hasHooked = true;
    //        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Hookable")
        {
            player.GetComponent<GrapplingHook>().hookedObject = other.gameObject;
            player.GetComponent<GrapplingHook>().hasHooked = true;
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
}
