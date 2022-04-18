using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BriefCasePointerObject : MonoBehaviour
{

    GameObject pointer;
    [SerializeField]
    GameObject pointerPosition;
    GameObject briefCase;
    // Start is called before the first frame update
    void Start()
    {
        briefCase = GameObject.FindGameObjectWithTag("BriefCase");
        pointer = GameObject.FindGameObjectWithTag("Pointer");
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.GetPhotonView().IsMine)
            return;

        pointer.transform.position = pointerPosition.transform.position;
        pointer.transform.LookAt(briefCase.transform);
    }
}
