using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StealBriefCase : MonoBehaviourPun
{
    public static float winTime = 30f;
    public static bool ownBriefcase;

    public GameObject briefCase;
    private GameObject gameManager;

    float maxStealTime = 1f;

    bool stealingBriefCase;
    float ownedTime;
    float stealTimer;
    bool gameOver;

    [SerializeField]
    float stealDistance;

    [SerializeField]
    GameObject briefCaseLocation;

    // Start is called before the first frame update
    void Start()
    {
        stealTimer = 0f;
        ownedTime = 0;
        gameOver = false;
        stealingBriefCase = false;
        ownBriefcase = false;
        briefCase = GameObject.FindGameObjectWithTag("BriefCase");
        gameManager = GameObject.FindGameObjectWithTag("GameManager");
    }

    // Update is called once per frame
    void Update()
    {
        //Break out of update loop if not the owner of this gameobject.
        if (!gameObject.GetPhotonView().IsMine)
            return; 
        
        if (ownBriefcase && !gameOver)
        {
            ownedTime += Time.deltaTime;
            if(ownedTime >= winTime)
            {
                gameOver = true;
                Debug.Log("Win");
                gameManager.GetPhotonView().RPC("EndGame", RpcTarget.All, photonView.Owner.NickName);
            }
        }
        else if(briefCase.GetComponent<BriefCase>().stealable == true)
        {
            if(Vector3.Distance(transform.position, briefCase.transform.position) <= stealDistance && Input.GetKeyDown(KeyCode.E))
            {
                //if(briefCase.transform.parent)
                //{
                //    if(briefCase.transform.parent.tag == "Player")
                //    {
                //        briefCase.transform.parent.GetComponent<StealBriefCase>().ownBriefcase = false;
                //    }
                //}
                //briefCase.transform.parent = null;

                briefCase.GetPhotonView().RequestOwnership();
                photonView.RPC("BriefcaseStolen", RpcTarget.All);
                
                stealingBriefCase = true;
                ownBriefcase = true;

                Debug.Log("ownBriefcase = true;");
            }
        }

        if(stealingBriefCase)
        {
            stealTimer += Time.deltaTime;
            float briefcaseDistance = (briefCase.transform.position - briefCaseLocation.transform.position).magnitude;
            briefCase.transform.position = Vector3.Lerp(briefCase.transform.position, briefCaseLocation.transform.position, Mathf.Max(20, 20 * briefcaseDistance) * Time.deltaTime);
            briefCase.transform.rotation = Quaternion.Lerp(briefCase.transform.rotation, briefCaseLocation.transform.rotation, 1);

            if(Vector3.Distance(briefCase.transform.position, briefCaseLocation.transform.position) < 0.1f || stealTimer >= maxStealTime)
            {
                briefCase.transform.position = briefCaseLocation.transform.position;
                briefCase.transform.rotation = briefCaseLocation.transform.rotation;
                briefCase.transform.parent = transform;
                stealingBriefCase = false;
                stealTimer = 0f;
            }
        }
    }

    [PunRPC]
    public void BriefcaseStolen()
    {
        briefCase.transform.parent = null;
        briefCase.GetComponent<BriefCase>().ResetStolenTimer();
        ownBriefcase = false;

        Debug.Log("ownBriefcase = false;");
    }
}
