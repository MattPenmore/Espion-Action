using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class StealBriefCase : MonoBehaviourPun
{
    public static float winTime = 60f;
    public static bool ownBriefcase;

    public GameObject briefCase;
    private GameObject gameManager;
    private GameObject playerTimerText;

    float maxStealTime = 1f;

    bool stealingBriefCase;
    public float ownedTime;
    float stealTimer;
    bool gameOver;

    [SerializeField]
    float stealDistance;

    [SerializeField]
    GameObject briefCaseLocationXZ;

    [SerializeField]
    GameObject briefCaseLocationY;

    [SerializeField]
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        stealTimer = 0f;
        ownedTime = winTime;
        gameOver = false;
        stealingBriefCase = false;
        ownBriefcase = false;
        briefCase = GameObject.FindGameObjectWithTag("BriefCase");
        gameManager = GameObject.FindGameObjectWithTag("GameManager");
        playerTimerText = GameObject.Find("PlayerTimerText");
    }

    // Update is called once per frame
    void Update()
    {
        //Break out of update loop if not the owner of this gameobject.
        if (!gameObject.GetPhotonView().IsMine)
            return;

        if (ownedTime > winTime)
            ownedTime = winTime;

        playerTimerText.GetComponent<Text>().text = (ownedTime).ToString("F1");

        if (ownBriefcase && !gameOver)
        {
            anim.SetBool("HasBriefCase", true);
            ownedTime -= Time.deltaTime;
            if(ownedTime <= 0)
            {
                gameOver = true;
                Debug.Log("Win");
                Vector4 pColour = GetComponentInChildren<Renderer>().material.color;
                float[] playerColour = { pColour.x, pColour.y, pColour.z, pColour.w };
                gameManager.GetPhotonView().RPC("EndGame", RpcTarget.All, photonView.Owner.NickName, playerColour);
            }
        }
        else if(briefCase.GetComponent<BriefCase>().stealable == true)
        {
            anim.SetBool("HasBriefCase", false);
            if (Vector3.Distance(transform.position, briefCase.transform.position) <= stealDistance && Input.GetKeyDown(KeyCode.E))
            {
                //if(briefCase.transform.parent)
                //{
                //    if(briefCase.transform.parent.tag == "Player")
                //    {
                //        briefCase.transform.parent.GetComponent<StealBriefCase>().ownBriefcase = false;
                //    }
                //}
                //briefCase.transform.parent = null;


                //briefCase.GetPhotonView().RequestOwnership();
                //photonView.RPC("BriefcaseStolen", RpcTarget.All);
                
                photonView.RPC(nameof(TransferBriefcase), RpcTarget.MasterClient, photonView.OwnerActorNr);
                
                //stealingBriefCase = true;
                //ownBriefcase = true;

                //Debug.Log("ownBriefcase = true;");
            }
        }
        else
            anim.SetBool("HasBriefCase", false);
        Vector3 briefCasePosition = new Vector3(briefCaseLocationXZ.transform.position.x, briefCaseLocationY.transform.position.y, briefCaseLocationXZ.transform.position.z);
        if (stealingBriefCase)
        {
            stealTimer += Time.deltaTime;
            float briefcaseDistance = (briefCase.transform.position - briefCasePosition).magnitude;
            briefCase.transform.position = Vector3.Lerp(briefCase.transform.position, briefCasePosition, Mathf.Max(20, 20 * briefcaseDistance) * Time.deltaTime);
            briefCase.transform.rotation = Quaternion.Lerp(briefCase.transform.rotation, briefCaseLocationXZ.transform.localRotation, 1);

            if(Vector3.Distance(briefCase.transform.position, briefCasePosition) < 0.1f || stealTimer >= maxStealTime)
            {
                briefCase.transform.position = briefCasePosition;
                briefCase.transform.rotation = briefCaseLocationXZ.transform.localRotation;
                briefCase.transform.parent = transform;
                stealingBriefCase = false;
                stealTimer = 0f;
            }
        }
        else if(ownBriefcase)
        {
            briefCase.transform.position = briefCasePosition;
            briefCase.transform.localRotation = briefCaseLocationXZ.transform.localRotation;
        }
    }

    [PunRPC]
    public void TransferBriefcase(int actorNo)
    {
        briefCase.GetPhotonView().TransferOwnership(actorNo);
        photonView.RPC(nameof(BriefcaseStolen), RpcTarget.AllBufferedViaServer, actorNo);
    }

    [PunRPC]
    public void BriefcaseStolen(int actorNo)
    {
        briefCase.transform.parent = null;
        briefCase.GetComponent<BriefCase>().ResetStolenTimer();
        ownBriefcase = false;

        string logMsg = "ownBriefcase = false;";
        
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNo)
        {
            stealingBriefCase = true;
            ownBriefcase = true;
            logMsg = "ownBriefcase = true;";
        }

        Debug.Log(logMsg);
    }
}
